namespace Neat.Util {
    public interface Iterator<E> {

        bool HasNext();

        E MoveNext();

        void Remove();

    }
}