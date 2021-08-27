using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool CheckAllocation(int leaveTypeId, string employeeId)
        {
            var period = DateTime.Now.Year;
            return FindAll().Where(s => s.EmployeeId == employeeId && s.LeaveTypeId == leaveTypeId && s.Period == period).Any();
        }

        public bool Create(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Add(entity);
            return Save();
        }

        public bool Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);
            return Save();
        }

        public ICollection<LeaveAllocation> FindAll()
        {
            return _db.LeaveAllocations
                .Include(s=>s.LeaveType)
                .Include(s=>s.Employee)
                .ToList();
        }

        public LeaveAllocation FindByID(int id)
        {
            return _db.LeaveAllocations
                .Include(s => s.LeaveType)
                .Include(s => s.Employee)
                .FirstOrDefault(s=>s.Id==id);

        }

        public ICollection<LeaveAllocation> GetLeaveAllocationsByEmployee(string id)
        {
            var period = DateTime.Now.Year;
            return FindAll().Where(s => s.EmployeeId == id && s.Period==period).ToList();
        }

        public bool isExists(int id)
        {
            var exists = _db.LeaveTypes.Any(s => s.Id == id);
            return exists;
        }

        public bool Save()
        {
            return _db.SaveChanges() > 0;
        }

        public bool Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);
            return Save();
        }
    }
}
