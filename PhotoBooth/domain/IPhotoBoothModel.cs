using System;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// Interface for the Photobooth application model.
    /// <para>
    /// The model maintains and controls the state of the photobooth by
    /// <list>
    /// <item>Providing a consistent workflow for the photobooth by handling event thrown by the hardware controls
    /// and by maintaining the correct state of the controls</item>
    /// <item>Maintaining data generated by the photobooth</item>
    /// </list>
    /// The model uses the Photobooth service to execute capturing and printing of pictures.
    /// </para>
    /// </summary>
    public interface IPhotoBoothModel: IDisposable
    {
        /// <summary>
        /// Event to signal that shutdown is requested
        /// </summary>
        /// <remarks>This event is added to provide a single location for handling all hardware controls, while keeping
        /// the responsibility of stopping the model at the application class</remarks>
        event EventHandler ShutdownRequested;

        /// <summary>
        /// Start the photobooth model
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the photobooth model
        /// </summary>
        void Stop();
    }
}