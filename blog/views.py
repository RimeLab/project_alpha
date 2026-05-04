from django.shortcuts import get_object_or_404, render

from .models import Post


def post_list(request):
    posts = Post.objects.select_related('created_by').all()
    return render(request, 'blog/post_list.html', {'posts': posts})


def post_detail(request, pk):
    post = get_object_or_404(Post.objects.select_related('created_by'), pk=pk)
    return render(request, 'blog/post_detail.html', {'post': post})
