using System;
using System.Collections.Generic;

namespace DbGitAddIn.ViewModel
{
    public class Comments<T>
    {
        private Queue<T> comments;

        public delegate void AddCommentHandler(Queue<T> comments);

        public event AddCommentHandler AddComment;

        private static object locker = new object();

        public Comments()
        {
            this.comments = new Queue<T>();
        }

        public void Add(T comment)
        {
            lock (locker)
            {
                comments.Enqueue(comment);
                AddComment?.Invoke(comments);
            }
        }
    }
}