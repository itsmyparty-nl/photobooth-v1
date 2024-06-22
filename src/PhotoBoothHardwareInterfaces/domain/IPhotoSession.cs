using SixLabors.ImageSharp;

namespace com.prodg.photobooth.domain;

public interface IPhotoSession
{
    string EventId { get; }
    int Id { get; }
    string StoragePath { get; }
    List<Image> Images { get; }
    Image? ResultImage { get; set; }
    int ImageCount { get; }
}