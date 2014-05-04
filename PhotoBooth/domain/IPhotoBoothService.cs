using System;
using System.Threading.Tasks;

namespace com.prodg.photobooth.domain
{
    public interface IPhotoBoothService
    {
        event EventHandler<PictureAddedEventArgs> PictureAdded;

        /// <summary>
        /// Capture a photo sessio
        /// </summary>
        /// <returns>The session containing the captured and processed images</returns>
        Task<PhotoSession> Capture();

        /// <summary>
        /// Print a photo session
        /// </summary>
        /// <param name="session"></param>
        void Print(PhotoSession session);
    }
}