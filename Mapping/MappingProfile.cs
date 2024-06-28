﻿using AutoMapper;
using lms.api.Models;
using lms.api.Models.RequestModels;

namespace lms.api.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Employees, CreateEmployeeRequest>().ReverseMap();
            CreateMap<Departments, CreateDepartmentRequest>().ReverseMap();
            CreateMap<Managers, CreateManagerRequest>().ReverseMap();
            CreateMap<Usermaster, CreateManagerRequest>().ReverseMap();
            CreateMap<Usermaster, CreateEmployeeRequest>().ReverseMap();
        }
    }
}
