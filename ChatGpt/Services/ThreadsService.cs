using ChatGpt.Data;
using Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Thread = Grpc.Thread;

namespace ChatGpt.Services;

public class ThreadsService : Grpc.ThreadsService.ThreadsServiceBase
{
    private readonly MessagingContext dbContext;

    public ThreadsService(MessagingContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override async Task<ListThreadsResponse> ListThreads(ListThreadsRequest request, ServerCallContext context)
    {
        var response = new ListThreadsResponse();

        var threads = await dbContext.Threads.Select(thread => new Thread
        {
            Id = thread.Id,
            Name = thread.Name
        }).ToListAsync();

        response.Threads.AddRange(threads);

        return response;
    }
}