using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neat.Util {
    public class ArrayList<E> : IEnumerable<E> {

        private E[] arr;
        public int size { get; private set; }
        public int capacity => arr.Length;

        public ArrayList(int initialCapacity = 10) {
            arr = new E[initialCapacity];
        }
        
        // * ----- MUTATOR SHIT ----- * //

        public void Add(E element) {
            if (size >= capacity)
                Resize();
            
            this[size] = element;
            size++;
        }

        public bool Remove(E element) {
            int index = this[element];
            if (index == -1)
                return false;

            for (int i = index; i < size - 1; i++)
                this[i] = this[i + 1];
            
            this[size - 1] = default;
            return true;
        }

        private void Resize() {
            E[] temp = new E[capacity * 2];
            for (int i = 0; i < size; i++) {
                temp[i] = arr[i];
            }

            arr = temp;
        }

        // * ----- OPERATOR SHIT ----- * //

        public E this[int i] {
            get => arr[i];
            set => arr[i] = value;
        }

        public int this[E element] {
            get {
                for (int i = 0; i < size; i++)
                    if (EqualityComparer<E>.Default.Equals(arr[i], element))
                        return i;
                return -1;
            }
        }
        
        // * ----- ENUMERATOR SHIT ----- * //
        
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<E> GetEnumerator() {
            return new ArrayListEnumerator(this);
        }

        private class ArrayListEnumerator : IEnumerator<E> {

            private readonly ArrayList<E> list;
            private int currentIndex;
            private bool removed;
            
            public ArrayListEnumerator(ArrayList<E> list) {
                this.list = list;
            }

            public void Dispose() {
                if (removed)
                    throw new Exception("Tried to remove twice?");

                removed = true;
                for (int i = currentIndex; i < list.size - 1; i++) {
                    list[i] = list[i + 1];
                }

                list[list.size - 1] = default;
            }

            public bool MoveNext() {
                if (removed) removed = false;
                else currentIndex++;

                return currentIndex < list.size;
            }

            public void Reset() {
                currentIndex = 0;
                removed = false;
            }

            object IEnumerator.Current => Current;

            public E Current => list[currentIndex];
        }

        protected bool Equals(ArrayList<E> other) {
            return arr.Equals(other.arr);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArrayList<E>) obj);
        }
    }
}