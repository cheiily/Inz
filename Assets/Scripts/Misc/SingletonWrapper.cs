namespace Misc {
    public class SingletonWrapper<T> {
        private T val;

        public delegate T Producer();

        private Producer producer;

        public T Get() {
            if ( val == null )
                val = producer();
            return val;
        }

        public SingletonWrapper(Producer producer) {
            this.producer = producer;
        }

        public static implicit operator T(SingletonWrapper<T> s) => s.Get();
    }
}