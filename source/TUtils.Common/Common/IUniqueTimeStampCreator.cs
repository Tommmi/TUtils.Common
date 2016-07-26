namespace TUtils.Common.Common
{
	/// <summary>
	/// a timestamp creator which creates process-unique timestamps
	/// </summary>
	public interface IUniqueTimeStampCreator
	{
		long Create();
	}
}