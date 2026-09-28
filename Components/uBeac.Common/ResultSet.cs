using System;
using System.Collections.Generic;

namespace uBeac
{
    public interface IResultSet<TResult> : IResultSet
    {
        TResult Data { get; set; }
    }

    public interface IResultSet
    {
        ResponseCodes Code { get; set; }
        List<Error> Errors { get; }
    }

    public class ResultSet<TResult> : IResultSet<TResult>
    {
        public ResponseCodes Code { get; set; }
        public TResult Data { get; set; }
        public List<Error> Errors { get; }

        public ResultSet()
        {
            Code = ResponseCodes.OK;
            Errors = new List<Error>();
        }

        public ResultSet(TResult value, ResponseCodes responseCode) : this()
        {
            //if (value != null)
            //{
            //    Type parameterType = value.GetType();
            //    if(parameterType == typeof(bool) && !Convert.ToBoolean(value))
            //        Code = ResponseCodes.BadRequest;
            //    else if (parameterType == typeof(Guid) && Guid.Parse(value.ToString()) == Guid.Empty)
            //        Code = ResponseCodes.BadRequest;
            //    else
            //        Code = ResponseCodes.OK;
            //}
            //else
            //    Code = ResponseCodes.NotFound;
            Code = responseCode;
            Data = value;
            Errors = new List<Error>();            
        }

        public ResultSet(ResponseCodes responseCode) : this()
        {
            Code = responseCode;
            Errors = new List<Error>();
        }

        public ResultSet(TResult value) : this()
        {
            Code = ResponseCodes.OK;
            Data = value;
            Errors = new List<Error>();
        }

        public ResultSet(Exception exception) : this()
        {
            Code = ResponseCodes.UnhandledException;
            Errors.Add(new Error(exception));
        }

        public void AddError(Error error)
        {
            Code = ResponseCodes.BadRequest;
            Errors.Add(error);
        }

        public void AddErrors(ICollection<Error> errors)
        {
            Code = ResponseCodes.BadRequest;
            Errors.AddRange(errors);
        }

        public void AddError(Exception ex)
        {
            Code = ResponseCodes.BadRequest;
            Errors.Add(new Error(ex));
        }

    }
}
