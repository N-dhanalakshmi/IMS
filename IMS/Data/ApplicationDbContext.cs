using Microsoft.EntityFrameworkCore;
using IMS.Models;
namespace IMS.Data;

public class ApplicationDbContext:DbContext
{
 
public  ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options){}
public DbSet<Policy> Policy{get;set;}
public DbSet<Help> Help{get;set;}
public DbSet<Response> Response{get;set;}
public DbSet<Dues> Dues {get;set;}
public DbSet<ClaimNow> ClaimNow{get;set;}
public DbSet<PolicyRequests> PolicyRequests{get;set;}

}