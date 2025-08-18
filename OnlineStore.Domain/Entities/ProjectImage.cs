namespace OnlineStore.Domain.Entities
{
    /// <summary>
    /// Изображение, привязанное к проекту
    /// </summary>
    public class ProjectImage
    {
        // Уникальный идентификатор изображения
        public Guid Id { get; set; }

        // Путь к изображению
        public string ImageUrl { get; set; } = null!;

        // Описание изображения
        public string? Description { get; set; }

        // Порядок отображения в галерее
        public int Order { get; set; }

        // Является ли изображение обложкой
        public bool IsPreview { get; set; }

        // Внешний ключ к проекту
        public Guid ProjectId { get; set; }

        // Навигационное свойство к проекту
        public Project Project { get; set; } = null!;
    }
}