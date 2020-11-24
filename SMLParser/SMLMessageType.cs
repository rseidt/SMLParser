using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public enum SMLMessageType
    {
        OpenRequest = 0x0100,
        OpenResponse = 0x0101,
        CloseRequest = 0x0201,
        CloseResponse = 0x0201,
        GetProfilePackRequest = 0x0300,
        GetProfilePackResponse = 0x0301,
        GetProfileListRequest = 0x0400,
        GetProfileListResponse = 0x0401,
        GetProcParameterRequest = 0x0500,
        GetProcParameterResponse = 0x0501,
        SetProcParameterRequest = 0x0500,
        GetListRequest = 0x0700,
        GetListResponse = 0x0701,
        GetCosemRequest = 0x0800,
        GetCosemResponse = 0x0801,
        SetCosemRequest = 0x0900,
        SetCosemResponse = 0x0901,
        ActionCosemRequest = 0x0A00,
        ActionCosemResponse = 0x0A01,
        AttensionResponse = 0xFF01 
    }
}
