namespace ECO.BLL.DTO.CategoryDtos
{
    public record CategoryDto(
        int Id,
        string Name,
        string Description,
        int? ParentCategoryId,
        string? ParentCategoryName
    );
    public record UpdateCategoryDto(
         string Name,
         string Description,
         int? ParentCategoryId,
         int Id
     );
}
