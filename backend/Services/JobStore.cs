using System.Collections.Concurrent;
using StackOverGrave.Api.Models;

namespace StackOverGrave.Api.Services;

/// <summary>
/// In-memory storage for repository conversion jobs
/// Thread-safe implementation using ConcurrentDictionary
/// </summary>
public class JobStore
{
    private static readonly ConcurrentDictionary<Guid, RepositoryJob> _jobs = new();

    /// <summary>
    /// Adds a new job to the store
    /// </summary>
    public static void AddJob(RepositoryJob job)
    {
        _jobs.TryAdd(job.Id, job);
    }

    /// <summary>
    /// Gets a job by ID, returns null if not found
    /// </summary>
    public static RepositoryJob? GetJob(Guid id)
    {
        return _jobs.TryGetValue(id, out var job) ? job : null;
    }

    /// <summary>
    /// Updates a job using an action delegate
    /// </summary>
    public static void UpdateJob(Guid id, Action<RepositoryJob> update)
    {
        if (_jobs.TryGetValue(id, out var job))
        {
            lock (job) // Ensure thread-safe updates
            {
                update(job);
            }
        }
    }

    /// <summary>
    /// Removes a job from the store
    /// </summary>
    public static void RemoveJob(Guid id)
    {
        _jobs.TryRemove(id, out _);
    }

    /// <summary>
    /// Gets all jobs (for monitoring/debugging)
    /// </summary>
    public static IEnumerable<RepositoryJob> GetAllJobs()
    {
        return _jobs.Values.ToList();
    }

    /// <summary>
    /// Gets the count of active jobs
    /// </summary>
    public static int GetActiveJobCount()
    {
        return _jobs.Count(j => j.Value.Status != JobStatus.Completed && j.Value.Status != JobStatus.Failed);
    }
}
