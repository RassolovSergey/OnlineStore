using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineStore.Application.Interfaces.Services
{
    public interface IUsersService
    {
        // Добавить пользователи права администратора
        Task MakeAdminAsync(Guid userId, CancellationToken ct = default);
        // Забрать у пользователя права администратора
        Task RemoveAdminAsync(Guid userId, CancellationToken ct = default);
        // Мягкое удаление пользователя (без физического удаления из БД)
        Task SoftDeleteAsync(Guid userId, CancellationToken ct = default);
        // Восстановление пользователя
        Task RestoreAsync(Guid userId, CancellationToken ct = default);
    }
}
