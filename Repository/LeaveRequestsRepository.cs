using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class LeaveRequestsRepository : ILeaveRequestsRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveRequestsRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<bool> Create(LeaveRequest entity)
        {
           await _db.LeaveRequests.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveRequest>> FindAll()
        {
            return await _db.LeaveRequests
                .Include(s=>s.RequestingEmployee)
                .Include(s=>s.ApprovedBy)
                .Include(s=>s.LeaveType)
                .ToListAsync();
        }

        public async Task<LeaveRequest> FindByID(int id)
        {
            return await _db.LeaveRequests
                .Include(s => s.RequestingEmployee)
                .Include(s => s.ApprovedBy)
                .Include(s => s.LeaveType)
                .FirstOrDefaultAsync(s=>s.Id==id);
        }

        public async Task<ICollection<LeaveRequest>> GetLeaveAllocationsByEmployee(string employeeId)
        {
            return await _db.LeaveRequests
                .Include(s => s.RequestingEmployee)
                .Include(s => s.ApprovedBy)
                .Include(s=>s.LeaveType)
                .Where(s => s.RequestingEmployeeId == employeeId).ToListAsync();
        }

        public async Task<bool> isExists(int id)
        {
            var exists = await _db.LeaveTypes.AnyAsync(s => s.Id == id);
            return exists;
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return await Save();
        }
    }
}
