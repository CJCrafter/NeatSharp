using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace Neat.Util {
    public class SortedSet<E> : IEnumerable<E> where E : class, IComparable<E> {
        private const int DEFAULT_INITIAL_CAPACITY = 1 << 4;
        private const int MAXIMUM_CAPACITY = 1 << 30;
        private const float DEFAULT_LOAD_FACTOR = 0.75f;

        private class Node<T> {
            internal T item;
            internal int hash;
            internal Node<T> last;
            internal Node<T> next;
            internal Node<T> bucket;

            public Node(T item, int hash) {
                this.item = item;
                this.hash = hash;
            }

            public Node(T item, int hash, Node<T> last, Node<T> next) {
                this.item = item;
                this.hash = hash;
                this.last = last;
                this.next = next;
            }

            public override string ToString() {
                return item.ToString();
            }
        }

        private Node<E>[] table;
        private Node<E> head;
        private Node<E> tail;
        public int threshold { get; private set; }
        public int size { get; private set; }
        public float loadFactor { get; }
        public int slowOperations { get; private set; }

        public SortedSet(int size = DEFAULT_INITIAL_CAPACITY, float loadFactor = DEFAULT_LOAD_FACTOR) {
            this.loadFactor = loadFactor;
            this.threshold = TableSize(size);
        }

        private int Hash(object item) => item.GetHashCode();

        private int Index(int hash) => hash & (table.Length - 1);

        private static int TableSize(int capacity) {
            if (capacity == 0)
                return 1;

            // Compute the next highest power of 2
            int n = capacity - 1;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;
            return n > MAXIMUM_CAPACITY ? MAXIMUM_CAPACITY : n;
        }

        
        // * ----- INTERNAL MAPPING METHODS ----- * //

        
        private Node<E> Get(object item) {
            if (Empty())
                return null;

            int hash = Hash(item);
            int index = Index(hash);
            Node<E> node = table[index];

            // Handle collisions (elements with a matching hash stored in bucket)
            if (node != null) {
                do {
                    if (hash == node.hash && item == node.item)
                        return node;
                } while ((node = node.bucket) != null);
            }

            return null;
        }

        private Node<E> RemoveTable(object item) {
            if (Empty())
                return null;

            int hash = Hash(item);
            int index = Index(hash);
            Node<E> p = table[index];

            if (p == null) {
                return null;
            }

            Node<E> node = null;
            if (hash == p.hash && item == p.item) {
                node = p;
            }

            Node<E> e;
            if ((e = p.bucket) != null) {
                do {
                    if (e.hash == hash && e.item == item) {
                        node = e;
                        break;
                    }

                    p = e;
                } while ((e = e.next) != null);
            }

            if (node != null) {
                if (node == p)
                    table[index] = node.bucket;
                else
                    p.next = node.next;

                --size;
                return node;
            }

            return null;
        }

        private void AddTable(Node<E> node) {
            if (table == null)
                Resize();

            int index = Index(node.hash);
            Node<E> bucket = table[index];
            if (bucket == null)
                table[index] = node;
            else
                throw new Exception("Hashing function must be wrong");

            if (++size > threshold)
                Resize();
        }

        private void Resize() {
            Node<E>[] oldTab = table;
            int oldCap = oldTab?.Length ?? 0;
            int oldThr = threshold;
            int newCap;
            int newThr = 0;
            if (oldCap > 0) {
                if (oldCap >= MAXIMUM_CAPACITY) {
                    threshold = int.MaxValue;
                    return;
                }
                else if ((newCap = oldCap << 1) < MAXIMUM_CAPACITY &&
                         oldCap >= DEFAULT_INITIAL_CAPACITY)
                    newThr = oldThr << 1; // double threshold
            }
            else if (oldThr > 0) // initial capacity was placed in threshold
                newCap = oldThr;
            else {
                // zero initial threshold signifies using defaults
                newCap = DEFAULT_INITIAL_CAPACITY;
                newThr = (int) (DEFAULT_LOAD_FACTOR * DEFAULT_INITIAL_CAPACITY);
            }

            if (newThr == 0) {
                float ft = (float) newCap * loadFactor;
                newThr = (newCap < MAXIMUM_CAPACITY && ft < (float) MAXIMUM_CAPACITY ? (int) ft : int.MaxValue);
            }

            threshold = newThr;

            Node<E>[] newTab = new Node<E>[newCap];
            table = newTab;
            if (oldTab != null) {
                for (int j = 0; j < oldCap; ++j) {
                    Node<E> node;
                    if ((node = oldTab[j]) != null) {
                        oldTab[j] = null;
                        if (node.bucket == null)
                            newTab[node.hash & (newCap - 1)] = node;
                        else {
                            Node<E> loHead = null, loTail = null;
                            Node<E> hiHead = null, hiTail = null;
                            Node<E> next;
                            do {
                                next = node.bucket;
                                if ((node.hash & oldCap) == 0) {
                                    if (loTail == null)
                                        loHead = node;
                                    else
                                        loTail.bucket = node;
                                    loTail = node;
                                }
                                else {
                                    if (hiTail == null)
                                        hiHead = node;
                                    else
                                        hiTail.bucket = node;
                                    hiTail = node;
                                }
                            } while ((node = next) != null);

                            if (loTail != null) {
                                loTail.bucket = null;
                                newTab[j] = loHead;
                            }

                            if (hiTail != null) {
                                hiTail.bucket = null;
                                newTab[j + oldCap] = hiHead;
                            }
                        }
                    }
                }
            }
        }


        // * ----- INTERNAL LINKING METHODS ----- * //

        
        private Node<E> LinkFirst(E item) {
            Node<E> first = head;
            head = new Node<E>(item, Hash(item), null, first);

            if (first == null)
                tail = head;
            else
                first.last = head;

            AddTable(head);
            return head;
        }

        private Node<E> LinkLast(E item) {
            Node<E> last = tail;
            tail = new Node<E>(item, Hash(item), last, null);

            if (last == null)
                head = tail;
            else
                last.next = tail;

            AddTable(tail);
            return tail;
        }

        private Node<E> LinkBefore(E item, Node<E> node) {
            Node<E> last = node.last;

            if (last == null)
                return LinkFirst(item);

            Node<E> newNode = new Node<E>(item, Hash(item), last, node);
            last.next = node.last = newNode;

            AddTable(newNode);
            return newNode;
        }

        private E UnlinkFirst() {
            if (Empty())
                return null;

            Node<E> first = head;
            if (first.next == null)
                head = tail = null;
            else {
                head = first.next;
                first.next.last = null;
            }

            RemoveTable(first.item);
            return first.item;
        }

        private E UnlinkLast() {
            if (Empty())
                return null;

            Node<E> last = tail;
            if (last.last == null)
                head = tail = null;
            else {
                tail = last.last;
                last.last.next = null;
            }

            RemoveTable(last.item);
            return last.item;
        }

        private E Unlink(Node<E> node) {
            Node<E> last = node.last;
            Node<E> next = node.next;

            if (last == null)
                head = next;
            else {
                last.next = next;
                node.last = null;
            }

            if (next == null)
                tail = last;
            else {
                next.last = last;
                node.next = null;
            }

            RemoveTable(node.item);
            return node.item;
        }

        
        // * ----- INDEX BASED OPERATIONS ----- * //

        
        private Node<E> Get(int index) {
            Node<E> node;
            slowOperations++;

            if (index < size >> 1) {
                node = head;
                for (int i = 0; i < index; i++)
                    node = node.next;
            }
            else {
                node = tail;
                for (int i = size - 1; i > index; i--)
                    node = node.last;
            }

            return node;
        }

        
        // * ----- PUBLIC ACCESSORS ----- * //

        
        public E this[E item] => Get(item)?.item;

        public E this[int index] => Get(index)?.item;

        public E first => head?.item;

        public E last => tail?.item;
        
        // TODO look into using table for random instead of linkedlist
        public E Random() {
            if (Empty())
                throw new Exception("No elements in table");

            return this[UnityEngine.Random.Range(0, size)];
        }

        public bool Contains(object item) => Get(item) != null;

        public bool Add(E item) {
            if (Contains(item))
                return false;

            LinkLast(item);
            return true;
        }

        public void Clear() {
            if (Empty())
                return;

            table = new Node<E>[table.Length];
        }

        public E Remove(E item) {
            Node<E> node = Get(item);
            return node == null ? null : Unlink(node);
        }

        public bool Empty() => size == 0;

        
        // * ----- SORTING METHODS ----- * //

        
        public bool AddSorted(E item) {
            if (Empty()) {
                LinkFirst(item);
                return true;
            }

            if (Contains(item))
                return false;

            Node<E> node = head;
            int compare;
            do {
                compare = node.item.CompareTo(item);
            } while (compare < 0 && (node = node.last) != null);

            if (node == null)
                LinkLast(item);
            else
                LinkBefore(item, node);

            return true;
        }

        public void Sort() {
            Split(head);
        }

        private static Node<E> Split(Node<E> start) {
            // No need to sort when empty or 1 element
            if (start?.next == null)
                return start;

            Node<E> middle = GetMiddle(start);
            Node<E> next = middle.next;

            // Unlink the 2 lists
            middle.next = null;
            next.last = null;

            Node<E> left = Split(start);
            Node<E> right = Split(next);
            return Merge(left, right);
        }

        private static Node<E> Merge(Node<E> a, Node<E> b) {
            Node<E> result = null;

            if (a == null)
                return b;
            if (b == null)
                return a;

            if (a.item.CompareTo(b.item) <= 0) {
                result = a;
                result.next = Merge(a.next, b);
                result.next.last = result;
            }
            else {
                result = b;
                result.next = Merge(a, b.next);
                result.next.last = result;
            }

            return result;
        }

        private static Node<E> GetMiddle(Node<E> start) {
            if (start == null)
                return null;

            Node<E> fast = start.next;
            Node<E> slow = start;

            while (fast != null) {
                fast = fast.next;
                if (fast != null) {
                    slow = slow.next;
                    fast = fast.next;
                }
            }

            return slow;
        }
        
        
        // * ----- ITERATOR METHODS ----- * //

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<E> GetEnumerator() {
            return new SortedSetEnumerator(this);
        }

        private class SortedSetEnumerator : IEnumerator<E> {

            private readonly SortedSet<E> parent;
            private Iterator<E> iterator;

            public SortedSetEnumerator(SortedSet<E> parent) {
                this.parent = parent;
                Reset();
            }

            public void Dispose() {
                iterator.Remove();
            }

            public bool MoveNext() {
                bool has = iterator.HasNext();
                if (has)
                    Current = iterator.MoveNext();

                return has;
            }

            public void Reset() {
                iterator = parent.Iterator();
            }

            object IEnumerator.Current => Current;

            public E Current { get; private set; }
        }

        public Iterator<E> Iterator() {
            return new SortedSetIterator(this);
        }

        private class SortedSetIterator : Iterator<E> {

            private readonly SortedSet<E> parent;
            private Node<E> current;
            private Node<E> delete;

            public SortedSetIterator(SortedSet<E> parent) {
                this.parent = parent;
            }

            public bool HasNext() {
                if (current == null && parent.head != null)
                    return true;

                if (delete != null)
                    return true;
                
                return current?.next != null;
            }

            public E MoveNext() {
                if (current == null)
                    return (current = parent.head).item;

                if (delete != null)
                    return (current = delete).item;
                
                return (current = current.next).item;
            }

            public void Remove() {
                delete = current.next;
                parent.Unlink(current);
            }
        }
    }
}