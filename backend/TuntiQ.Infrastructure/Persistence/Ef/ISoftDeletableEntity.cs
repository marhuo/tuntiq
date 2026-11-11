
namespace TuntiQ.Infrastructure.Persistence.Ef;

public interface ISoftDeletableEntity
{
    bool Deleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
