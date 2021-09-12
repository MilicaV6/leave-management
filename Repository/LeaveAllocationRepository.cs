using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace leave_management.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> CheckAllocation(int leaveTypeId, string employeeId)
        {
            var period = DateTime.Now.Year;
            var leaveAllocations = await FindAll();
            return leaveAllocations.Where(s => s.EmployeeId == employeeId && s.LeaveTypeId == leaveTypeId && s.Period == period).Any();
        }

        public async Task<bool> Create(LeaveAllocation entity)
        {
           await _db.LeaveAllocations.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveAllocation>> FindAll()
        {
            return await _db.LeaveAllocations
                .Include(s=>s.LeaveType)
                .Include(s=>s.Employee)
                .ToListAsync();
        }

        public async Task<LeaveAllocation> FindByID(int id)
        {
            return await _db.LeaveAllocations
                .Include(s => s.LeaveType)
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s=>s.Id==id);

        }

        public async Task<ICollection<LeaveAllocation>> GetLeaveAllocationsByEmployee(string id)
        {
            var period = DateTime.Now.Year;
            var leaveAllocations =await FindAll();
            return  leaveAllocations.Where(s => s.EmployeeId == id && s.Period==period).ToList();
        }

        public async Task<LeaveAllocation> GetLeaveAllocationsByEmployeeAndType(string employeeId, int leaveTypeId)
        {
            var period = DateTime.Now.Year;
            var leaveAllocations = await FindAll();
            return  leaveAllocations.FirstOrDefault(s => s.EmployeeId == employeeId && s.LeaveTypeId==leaveTypeId && s.Period == period);
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

        public async Task<bool> Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);
            return await Save();
        }
    }
}
