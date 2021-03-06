﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Business.Interfaces;
using DomainModel.RequestModels;
using WebAPI.Exceptions;
using Microsoft.AspNetCore.Http;
using System.IO;
using DomainModel.Models;
using DomainModel.ResponseModels;
using ContentDispositionHeaderValue = Microsoft.Net.Http.Headers.ContentDispositionHeaderValue;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class StudentController : Controller
    {
        protected ILogger _logger { get; }
        private IStudent _studentInfoService;

        public StudentController(ILoggerFactory loggerFactory, IStudent studentInfoService)
        {
            _logger = loggerFactory.CreateLogger(GetType().Namespace);
            _studentInfoService = studentInfoService;
        }

        [HttpGet("")]
        [HttpGet("id/{id?}")]
        public async Task<IActionResult> Get(short? id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            try
            {
                if (id.HasValue)
                {
                    var studentResponse = _studentInfoService.GetStudent(id.Value);

                    if (studentResponse.Result.Model == null) throw new NotFoundException();

                    var apiClient = new QMSApiClient
                    {
                        BaseURL = "http://localhost:5000",
                        REST_SERVICE_URL_PREFIX = "api/student"
                    };
                    var jsonResponse = apiClient.GetJsonResponse();
                    return Ok(Serialization<ListModelResponse<StudentsInfo>>.Serialize(jsonResponse));
                }

                else
                {
                    return Ok(await _studentInfoService.GetStudentsInfo(pageNumber, pageSize));
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]AddUpdateStudentRequestDTO request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _studentInfoService.AddStudentInfo(request));
                }
                else
                {
                    throw new BadRequestException(ModelState.ToList());
                }
            }
            catch (Exception e)
            {
                throw e;
                //_logger.LogError(e.Message, e);

                //return StatusCode(500, e.Message);
            }
        }

        [HttpPost("upload")]
        public async void UploadFiles(IList<IFormFile> files)
        {
            if (files.Count == 0) files = HttpContext.Request.Form.Files.ToList();

            long size = 0;
            foreach (var file in files)
            {
                var filename = ContentDispositionHeaderValue
                                .Parse(file.ContentDisposition)
                                .FileName
                                .Trim('"');
                size += file.Length;

                if (!System.IO.Directory.Exists("Uploads"))
                {
                    System.IO.DirectoryInfo directory = System.IO.Directory.CreateDirectory("Uploads");
                }

                using (FileStream fs = System.IO.File.Create("Uploads/" + file.FileName))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody]AddUpdateStudentRequestDTO request)
        {
            try
            {
                return Ok(await _studentInfoService.UpdateStudentInfo(request));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);

                return StatusCode(500, e.Message);
            }
        }

        // DELETE api/Hosts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                return Ok(await Task.FromResult(_studentInfoService.DeleteStudentInfo(id)));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);

                return StatusCode(500, e.Message);
            }
        }
    }
}
