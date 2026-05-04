from django.contrib.auth import get_user_model
from django.test import TestCase
from django.urls import reverse

from .models import Post

User = get_user_model()


class PostModelTest(TestCase):
    def setUp(self):
        self.user = User.objects.create_user(username='testuser', password='pass')

    def test_str(self):
        post = Post(title='Hello World', created_by=self.user)
        self.assertEqual(str(post), 'Hello World')

    def test_default_ordering_is_newest_first(self):
        Post.objects.create(title='First', body='', created_by=self.user)
        Post.objects.create(title='Second', body='', created_by=self.user)
        titles = list(Post.objects.values_list('title', flat=True))
        self.assertEqual(titles, ['Second', 'First'])


class PostListViewTest(TestCase):
    def setUp(self):
        self.user = User.objects.create_user(username='testuser', password='pass')
        self.url = reverse('blog:post_list')

    def test_returns_200(self):
        response = self.client.get(self.url)
        self.assertEqual(response.status_code, 200)

    def test_uses_correct_template(self):
        response = self.client.get(self.url)
        self.assertTemplateUsed(response, 'blog/post_list.html')

    def test_shows_all_posts(self):
        Post.objects.create(title='Post A', body='', created_by=self.user)
        Post.objects.create(title='Post B', body='', created_by=self.user)
        response = self.client.get(self.url)
        self.assertContains(response, 'Post A')
        self.assertContains(response, 'Post B')

    def test_empty_state(self):
        response = self.client.get(self.url)
        self.assertContains(response, 'No posts yet')


class PostDetailViewTest(TestCase):
    def setUp(self):
        self.user = User.objects.create_user(username='testuser', password='pass')
        self.post = Post.objects.create(
            title='My Post',
            body='Some body text.',
            created_by=self.user,
        )
        self.url = reverse('blog:post_detail', args=[self.post.pk])

    def test_returns_200(self):
        response = self.client.get(self.url)
        self.assertEqual(response.status_code, 200)

    def test_uses_correct_template(self):
        response = self.client.get(self.url)
        self.assertTemplateUsed(response, 'blog/post_detail.html')

    def test_shows_post_content(self):
        response = self.client.get(self.url)
        self.assertContains(response, 'My Post')
        self.assertContains(response, 'Some body text.')

    def test_shows_author(self):
        response = self.client.get(self.url)
        self.assertContains(response, 'testuser')

    def test_returns_404_for_missing_post(self):
        response = self.client.get(reverse('blog:post_detail', args=[99999]))
        self.assertEqual(response.status_code, 404)


class RootRedirectTest(TestCase):
    def test_root_redirects_to_blog(self):
        response = self.client.get('/')
        self.assertRedirects(response, '/blog/', fetch_redirect_response=False)
