using Xunit;

// Configure xUnit to run test collections in parallel
[assembly: CollectionBehavior(DisableTestParallelization = false, MaxParallelThreads = -1)]

