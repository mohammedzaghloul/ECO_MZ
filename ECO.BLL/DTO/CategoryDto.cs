namespace ECO.BLL.DTO
{
    public record CategoryDto(
        int Id,
        string Name,
        string Description
    );
    public record UpdateCategoryDto(
         string Name,
         string Description,
         int Id
     );
}
