namespace CrudApp.Infrastructure.UtilityCode;

public static class EntityBaseUtils
{
    public static T AssertNotDeleted<T>(this T entity) where T : EntityBase
    {
        if (entity.IsSoftDeleted)
            throw new ApiResponseException(HttpStatus.NotFound, $"{entity.DisplayName} has been deleted.");
        return entity;
    }
}
