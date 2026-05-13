namespace Inventra.Application.Interfaces
{
    public interface IInventoryPermissionService
    {
        Task<bool> CanWriteAsync(string userId, bool isAdmin, int inventoryId);
        Task<bool> CanManageAsync(string userId, bool isAdmin, int inventoryId);
    }
}
