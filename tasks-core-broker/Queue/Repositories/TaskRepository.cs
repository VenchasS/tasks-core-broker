using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using TaskQueue.Database;

namespace TaskQueue.Repositories
{
    public class TaskRepository
    {
        private readonly AppDbContext _dbContext;

        public TaskRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddTaskAsync(TaskItem task)
        {
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<TaskItem> GetTaskByIdAsync(int id)
        {
            return await _dbContext.Tasks.FindAsync(id);
        }

        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            return await _dbContext.Tasks.ToListAsync();
        }

        public async Task UpdateTaskAsync(TaskItem task)
        {
            _dbContext.Tasks.Update(task);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int id)
        {
            var task = await GetTaskByIdAsync(id);
            if (task != null)
            {
                _dbContext.Tasks.Remove(task);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}