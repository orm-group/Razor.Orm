﻿using Razor.Orm.Example.Dto;
using System.Collections.Generic;

namespace Razor.Orm.Example.Dao.TestDao
{
    public interface ITestDao
    {
        decimal SaveLocation(LocationDto dto);
        void UpdatePeople(PeopleFilterDto dto, string title);
        int CountPeople(PeopleFilterDto dto);
        PeopleDto GetSinglePeople(PeopleFilterDto dto);
        IEnumerable<PeopleDto> GetAllPeople(PeopleFilterDto dto);
    }
}
