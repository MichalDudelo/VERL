using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arena_Server.Infrastructure
{
    public class MovesQueue :
    System.Collections.Generic.List<Move>
    {
        private object SyncRoot = new object();

        public bool Enqueue(Move item)
        {
            lock (this.SyncRoot)
            {
                if (!this.Exists(el => el.Robot.Equals(item.Robot)))
                {
                    this.Add(item);
                    return true;
                }
                else
                    return false;
            }
        }

        public Move Dequeue()
        {
            Move item = default(Move);

            lock (this.SyncRoot)
            {
                if (this.Count > 0)
                {

                    try
                    {
                        if (this.Exists(m => m.MadeMove == Common_Library.Infrastructure.MoveType.Shoot))
                        {
                            var move = this.First(m => m.MadeMove == Common_Library.Infrastructure.MoveType.Shoot );
                            item = move;
                            this.RemoveAt(this.IndexOf(item));
                        }
                        else
                        {
                            item = this[0];
                            this.RemoveAt(0);
                        }

                    }
                    catch (InvalidOperationException)
                    {
                        item = this[0];
                        this.RemoveAt(0);
                    };

                }
            }

            return (item);
        }

        public void Push(Move item)
        {
            lock (this.SyncRoot)
            {
                this.Add(item);
            }
        }

        public Move Pop()
        {
            Move item = default(Move);

            lock (this.SyncRoot)
            {
                if (this.Count > 0)
                {
                    item = this[this.Count - 1];
                    this.RemoveAt(this.Count - 1);
                }
            }

            return (item);
        }

        public Move PeekQueue()
        {
            Move item = default(Move);

            lock (this.SyncRoot)
            {
                if (this.Count > 0)
                {
                    item = this[0];
                }
            }

            return (item);
        }

        public Move PeekStack()
        {
            Move item = default(Move);

            lock (this.SyncRoot)
            {
                if (this.Count > 0)
                {
                    item = this[this.Count - 1];
                }
            }

            return (item);
        }
    }
}
