namespace LibGPhoto2
{
	public class PortInfo : IPortInfo
	{
		public PortInfoHandle Handle { get; private set; }

		internal PortInfo (PortInfoHandle handle)
		{
		    Handle = handle;
		}
		
		public string Name {
			get
			{
			    return Handle.name;
			}
		}
		
		public string Path {
			get
			{
			    return Handle.path;
			}
		}
	}
}
