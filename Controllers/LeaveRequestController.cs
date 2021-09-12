using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestsRepository _leaveRequestRepo;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly ILeaveAllocationRepository _leaveAllocRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestController(ILeaveRequestsRepository leaveRequestRepo, ILeaveTypeRepository leaveTypeRepository, ILeaveAllocationRepository leaveAllocRepo, IMapper mapper,  UserManager<Employee> userManager)
        {
            _leaveRequestRepo = leaveRequestRepo;
            _leaveTypeRepo = leaveTypeRepository;
            _leaveAllocRepo = leaveAllocRepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        [Authorize(Roles = "Administrator")]
        // GET: LeaveRequestController
        public async Task<ActionResult> Index()
        {
            var leaveRequests =await _leaveRequestRepo.FindAll();
            var leaveRequestsModel = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);
            var model = new AdminLeaveRequestViewVM
            {
                TotalRequests = leaveRequestsModel.Count,
                ApprovedRequests = leaveRequestsModel.Count(s => s.Approved == true),
                PendingRequests = leaveRequestsModel.Count(s => s.Approved == null),
                RejectedRequests = leaveRequestsModel.Count(s => s.Approved == false),
                LeaveRequests=leaveRequestsModel
            };

            return View(model);
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var leaveRequest =await _leaveRequestRepo.FindByID(id);
            var model = _mapper.Map<LeaveRequestVM>(leaveRequest);

            return View(model);
        }

        public async Task<ActionResult> ApproveRequest(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var leaveRequest =await _leaveRequestRepo.FindByID(id);
                var allocation =await _leaveAllocRepo.GetLeaveAllocationsByEmployeeAndType(leaveRequest.RequestingEmployeeId,leaveRequest.LeaveTypeId);
                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;
                allocation.NumberOfDays = allocation.NumberOfDays - daysRequested;
                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;
                await _leaveRequestRepo.Update(leaveRequest);
                await _leaveAllocRepo.Update(allocation);
                return RedirectToAction(nameof(Index));
               
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

           
        }
        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                var user =await _userManager.GetUserAsync(User);
                var leaveRequest =await _leaveRequestRepo.FindByID(id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;
                await _leaveRequestRepo.Update(leaveRequest);
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

        }
        public async Task<ActionResult> CancelRequest(int id)
        {
            var leaveRequest =await _leaveRequestRepo.FindByID(id);
            leaveRequest.Cancelled = true;
            await _leaveRequestRepo.Update(leaveRequest);
            return RedirectToAction("MyLeave");
        }
        public async Task<ActionResult> MyLeave()
        {
            var employee =await _userManager.GetUserAsync(User);
            var employeeAllocations =await _leaveAllocRepo.GetLeaveAllocationsByEmployee(employee.Id);
            var employeeRequests =await _leaveRequestRepo.GetLeaveAllocationsByEmployee(employee.Id);

            var employeeAllocationsModel = _mapper.Map<List<LeaveAllocationVM>>(employeeAllocations);
            var employeeRequestsModel = _mapper.Map<List<LeaveRequestVM>>(employeeRequests);
            var model = new EmployeeLeaveRequestViewVM
            {
                LeaveAllocations = employeeAllocationsModel,
                LeaveRequests = employeeRequestsModel
            };
            return View(model);
        }

        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            var leaveTypes =await _leaveTypeRepo.FindAll();
            var leaveTypeItems = leaveTypes.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            });
            var model = new CreateLeaveRequestVM
            {
                LeaveTypes = leaveTypeItems
            };
            return View(model);
        }

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVM model)
        {
            try
            {
                var startDate =Convert.ToDateTime( model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);
                var leaveTypes =await _leaveTypeRepo.FindAll();
                var leaveTypeItems = leaveTypes.Select(s => new SelectListItem
                {
                    Text = s.Name,
                    Value = s.Id.ToString()
                });
                model.LeaveTypes = leaveTypeItems;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                if(DateTime.Compare(startDate,endDate) > 1)
                {
                    ModelState.AddModelError("", "Start date cannot be further in the future then the End date");
                    return View(model);
                }

                var employee = await _userManager.GetUserAsync(User);
                var allocation =await _leaveAllocRepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
                if (allocation == null)
                {
                    ModelState.AddModelError("", "You are not able to request this type of allocation");
                    return View(model);
                }

                int daysRequested = (int) (endDate - startDate).TotalDays;
                if (daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You do not have sufficient days for this request");
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestVM
                {
                    RequestingEmployeeId = employee.Id,
                    StartDate =startDate,
                    EndDate =endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId=model.LeaveTypeId,
                    RequestComments=model.RequestComments

                    
                };
                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                var isSuccess =await _leaveRequestRepo.Create(leaveRequest);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something Went Wrong with submitting your record");
                    return View(model);
                }

                return RedirectToAction("MyLeave");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Something Went Wrong");
                return View(model);
            }
        }

        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
