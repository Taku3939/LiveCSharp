// Created by Takuya Isaki on 2021/03/09

using System.Net;

namespace UI.infrastracture
{
    public interface IIPSetting
    {
        IPAddress GetTcpIp();
        IPAddress GetUdpIp();
        int GetTcpPort();
        int GetUdpPort();
    }
}