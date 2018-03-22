using System.Threading;

namespace LorenzoExtractor.Helpers
{
    /// <summary>
    /// Uses Interlocked.Exchange() to change the value, so that all threads see the same value.
    /// </summary>
    public class InterlockedBool
    {
        private int _value;
        public bool Value
        {
            // there won't ever be a need to change _value to a long.
            // https://stackoverflow.com/questions/6139699/how-to-correctly-read-an-interlocked-incremented-int-field
            //get {return Interlocked.CompareExchange(ref _value, 0, 0) > 0; }
            get { return this._value > 0; }
            set { Interlocked.Exchange(ref this._value, value ? 1 : 0); }
        }

        public InterlockedBool(bool value) { this.Value = value; }
        public InterlockedBool() : this(false) { }

        public static implicit operator bool(InterlockedBool obj)
        {
            return obj.Value;
        }
        public static implicit operator InterlockedBool(bool obj)
        {
            return new InterlockedBool(obj);
        }
        public static bool operator ==(InterlockedBool obj1, object obj2)
        {
            if (obj2 is bool)
                return obj1.Value.Equals((bool)obj2);
            if (obj2 is InterlockedBool)
                return obj1.Value.Equals(((InterlockedBool)obj2).Value);
            return false;
        }
        public static bool operator !=(InterlockedBool obj1, object obj2)
        {
            if (obj2 is bool)
                return obj1.Value.Equals((bool)obj2);
            if (obj2 is InterlockedBool)
                return obj1.Value.Equals(((InterlockedBool)obj2).Value);
            return false;
        }
        public override bool Equals(object obj)
        {
            if (obj is bool)
                return this.Value.Equals((bool)obj);
            if (obj is InterlockedBool)
                return this.Value.Equals(((InterlockedBool)obj).Value);
            return false;
        }
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
    }
}
