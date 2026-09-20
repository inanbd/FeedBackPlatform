using System.Data;

namespace FeedbackPlatform.Application.Common.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
