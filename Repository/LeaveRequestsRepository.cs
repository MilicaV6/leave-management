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
        public bool Create(LeaveRequest entity)
        {
            _db.LeaveRequests.Add(entity);
            return Save();
        }

        public bool Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return Save();
        }

        public ICollection<LeaveRequest> FindAll()
        {
            return _db.LeaveRequests
                .Include(s=>s.RequestingEmployee)
                .Include(s=>s.ApprovedBy)
                .Include(s=>s.LeaveType)
                .ToList();
        }

        public LeaveRequest FindByID(int id)
        {
            return _db.LeaveRequests
                .Include(s => s.RequestingEmployee)
                .Include(s => s.ApprovedBy)
                .Include(s => s.LeaveType)
                .FirstOrDefault(s=>s.Id==id);
        }

        public ICollection<LeaveRequest> GetLeaveAllocationsByEmployee(string employeeId)
        {
            return _db.LeaveRequests
                .Include(s => s.RequestingEmployee)
                .Include(s => s.ApprovedBy)
                .Include(s=>s.LeaveType)
                .Where(s => s.RequestingEmployeeId == employeeId).ToList();
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

        public bool Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return Save();
        }
    }
}
