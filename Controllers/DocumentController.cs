using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.Services;
using StudyResource.ViewModels.Document;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudyResource.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly GoogleDriveService _googleDriveService;

        public DocumentController(ApplicationDbContext context,
            UserManager<User> userManager,
            GoogleDriveService googleDriveService)
        {
            _context = context;
            _userManager = userManager;
            _googleDriveService = googleDriveService;
        }

        [HttpGet]
        public async Task<IActionResult> Ebook(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            string fileUrl = Url.Action("DownloadFile", "GoogleDrive", new { fileId = document.GoogleDriveId });
            ViewData["DefaultFileUrl"] = fileUrl;

            return View();
        }

        public IActionResult Index(string searchString, int? gradeId, int? setId)
        {
            var documents = _context.Documents
                .Include(d => d.GradeSubject.Grade)
                .Include(d => d.Set)
                .Include(d => d.DocumentType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                documents = documents.Where(d => d.Title.Contains(searchString));
            }

            if (gradeId.HasValue)
            {
                documents = documents.Where(d => d.GradeSubject.GradeId == gradeId);
                ViewBag.CurrentGradeId = gradeId;
            }
            else
            {
                ViewBag.CurrentGradeId = null;
            }

            if (setId.HasValue)
            {
                documents = documents.Where(d => d.SetId == setId);
                ViewBag.CurrentSetId = setId;
            }
            else
            {
                ViewBag.CurrentSetId = null;
            }

            ViewBag.Grades = _context.Grades.ToList();
            ViewBag.Sets = _context.Sets.ToList();

            return View(documents.ToList());
        }

        [HttpGet] public async Task<IActionResult> Detail(int id) 
        { 
            var document = await _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.Set)
                .FirstOrDefaultAsync(d => d.Id == id); 
            
            if (document == null) 
            { 
                return NotFound(); 
            } 
            var comments = await _context.UserComments
                .Where(c => c.DocumentId == id)
                .Select(c => new DocumentDetailViewModel.UserComment 
                {
                    Username = c.User != null ? c.User.UserName : "Anonymous",
                    Comment = c.Comment, Rating = c.Rating, CommentDate = c.CommentDate 
                })
                .ToListAsync(); var viewModel = new DocumentDetailViewModel 
                { 
                    Id = document.Id, 
                    Title = document.Title,
                    Slug = document.Slug,
                    Description = document.Description,
                    GoogleDriveId = document.GoogleDriveId, 
                    DocumentTypeId = document.DocumentTypeId, 
                    DocumentType = document.DocumentType, 
                    UserNotes = string.Empty, 
                    UserComments = comments }; 
            
            return View(viewModel); 
        }

        [HttpPost]
        public async Task<IActionResult> SubmitComment(string Comment, string GoogleDriveId)
        {
            if (ModelState.IsValid)
            {
                var document = await _context.Documents.FirstOrDefaultAsync(d => d.GoogleDriveId == GoogleDriveId); 
                if (document == null) 
                {
                    return NotFound(); 
                }

                var user = await _userManager.GetUserAsync(User); 
                
                var userComment = new UserComment
                {
                    UserId = user?.Id,
                    CommentDate = DateTime.Now,
                    Comment = Comment,
                    DocumentId = document.Id,
                    Rating = 0  
                };

                _context.UserComments
                    .Add(userComment); 
                await _context.SaveChangesAsync(); 

                return Json(new {
                    username = user?.UserName ?? "Anonymous", 
                    commentDate = userComment.CommentDate.ToShortDateString(),
                    comment = userComment.Comment }); 
            }
            
            return BadRequest(); }
        }
}