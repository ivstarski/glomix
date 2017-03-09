// ----------------------------------------------------------------------
// <copyright file="IWebBuilder.cs" company="Gazprom Space Systems">
// Copyright statement. All right reserved 
// Developer:   Ivan Starski
// Date: 17/02/2017 10:47
// </copyright>
// ----------------------------------------------------------------------
namespace sdk.builder
{
    using System.Threading.Tasks;
    using model;

    interface IWebBuilder
    {
        Task<IContent> Build(string url);
    }
}