namespace App.Core
{
    public abstract class BaseEntity<T>
    {
        public T Id { get; set; } = default!;
    }
}
