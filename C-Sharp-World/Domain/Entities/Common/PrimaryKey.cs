namespace IdentityVerificationService.Domain.Entities.Common
{
    public class PrimaryKey<T> : IEntity
    {
        public T Id { get; set; }

        public override string ToString() => Id.ToString();
    }
}