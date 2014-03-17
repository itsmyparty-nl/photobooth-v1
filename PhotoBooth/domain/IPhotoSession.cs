/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

namespace com.prodg.photobooth.domain
{
	public interface IPhotoSession
	{
		string Id { get;}
		
		string StoragePath { get; }
		
		void AddPicture(string path);
		
		string Finish();
	}
}
