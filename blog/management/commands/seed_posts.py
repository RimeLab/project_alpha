from django.contrib.auth import get_user_model
from django.core.management.base import BaseCommand

from blog.models import Post

User = get_user_model()

POSTS = [
    (
        "Getting Started with Django",
        "Django is a high-level Python web framework that encourages rapid development "
        "and clean, pragmatic design. Built by experienced developers, it takes care of "
        "much of the hassle of web development, so you can focus on writing your app "
        "without needing to reinvent the wheel. It's free and open source.\n\n"
        "To get started, install Django via pip and run django-admin startproject. "
        "From there you can create apps, define models, and build views in minutes.",
    ),
    (
        "PostgreSQL Tips for Django Developers",
        "Switching from SQLite to PostgreSQL unlocks features like full-text search, "
        "JSON fields, and better concurrency. Here are a few tips to make the most of it.\n\n"
        "Use select_related() and prefetch_related() to avoid N+1 queries. Take advantage "
        "of Django's JSONField for flexible schema-less data. And always run EXPLAIN ANALYZE "
        "on slow queries before adding indexes.",
    ),
    (
        "Writing Readable Code",
        "Readable code is code that communicates intent clearly, without relying on "
        "comments to explain what it does. A well-named function tells a story.\n\n"
        "Keep functions short and focused on a single task. Prefer explicit variable names "
        "over abbreviations. Consistency matters more than style — pick a convention and "
        "stick to it across the codebase.",
    ),
    (
        "Understanding Django's ORM",
        "Django's ORM lets you interact with your database using Python objects instead of "
        "raw SQL. QuerySets are lazy — they're only evaluated when you iterate over them, "
        "slice them, or call list() on them.\n\n"
        "Use .values() when you only need a subset of fields. Batch large operations with "
        "iterator() to avoid loading everything into memory at once.",
    ),
    (
        "Deploying Django with Docker",
        "Containerising a Django app makes deployments reproducible and environment "
        "differences disappear. A typical setup uses one container for the web process, "
        "one for PostgreSQL, and optionally one for a task queue.\n\n"
        "Keep secrets out of images — pass them via environment variables or a secrets "
        "manager. Use a multi-stage build to keep your production image lean.",
    ),
]


class Command(BaseCommand):
    help = 'Seed the database with sample blog posts'

    def handle(self, *args, **options):
        user, created = User.objects.get_or_create(
            username='admin',
            defaults={'email': 'admin@example.com', 'is_staff': True, 'is_superuser': True},
        )
        if created:
            user.set_password('admin')
            user.save()
            self.stdout.write(self.style.SUCCESS('Created superuser: admin / admin'))

        count = 0
        for title, body in POSTS:
            _, new = Post.objects.get_or_create(title=title, defaults={'body': body, 'created_by': user})
            if new:
                count += 1

        self.stdout.write(self.style.SUCCESS(f'Seeded {count} post(s).'))
