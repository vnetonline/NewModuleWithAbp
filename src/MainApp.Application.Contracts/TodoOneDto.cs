﻿using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace MainApp
{
    public class TodoOneDto : EntityDto<Guid>
    {
        public string Content { get; set; }
        public bool IsDone { get; set; }
    }
}
