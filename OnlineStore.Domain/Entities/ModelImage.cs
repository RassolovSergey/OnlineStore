namespace OnlineStore.Domain.Entities
{
    /// <summary>
    /// Изображение, привязанное к 3D-модели
    /// </summary>
    public class ModelImage
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

        // Внешний ключ к 3D-модели
        public Guid Model3DId { get; set; }

        // Навигационное свойство к 3D-модели
        public Model3D Model3D { get; set; } = null!;
    }
}