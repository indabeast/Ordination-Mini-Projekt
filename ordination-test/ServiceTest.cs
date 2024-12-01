namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;

[TestClass]
public class ServiceTest
{
    private DataService service;

    [TestInitialize]
    public void SetupBeforeEachTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdinationContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName: "test-database");
        var context = new OrdinationContext(optionsBuilder.Options);
        service = new DataService(context);
        service.SeedData();
    }

    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }

    [TestMethod]
    public void OpretDagligFast()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(2, service.GetDagligFaste().Count());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestAtKodenSmiderEnException()
    {
        // Herunder skal man så kalde noget kode,
        // der smider en exception.

        // Hvis koden _ikke_ smider en exception,
        // så fejler testen.

        Console.WriteLine("Her kommer der ikke en exception. Testen fejler.");
    }
    //General Ordrination testing 
    //TC1
    [TestMethod]
    public void ValidOrdinationDates_ShouldPass()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        var ordination = service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            1, 1, 1, 1, new DateTime(2024, 11, 1), new DateTime(2024, 11, 10));

        // Assert
        Assert.IsNotNull(ordination);
    }
    
    //TC2
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void InvalidOrdinationDates_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            1, 1, 1, 1, new DateTime(2024, 11, 10), new DateTime(2024, 11, 1));
    }

    //TC3
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void OrdinationLessThanOneDay_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            1, 1, 1, 1, new DateTime(2024, 11, 1), new DateTime(2024, 11, 1));
    }

    //TC4
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void InvalidUnitType_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();

        // Act
        var ordination = new DagligFast(new DateTime(2024, 11, 1), new DateTime(2024, 11, 10),
            new Laegemiddel("InvalidMed", 1, 1, 1, "kapsler"), 1, 1, 1, 1);
    }

    //TC5
  


    
    // Test af gyldige PN
    //TC1
    [TestMethod]
    public void ValidPNOrdination_ShouldPass()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        var pn = service.OpretPN(patient.PatientId, lm.LaegemiddelId, 2, new DateTime(2024, 11, 3), new DateTime(2024, 11, 10));

        // Assert
        Assert.IsNotNull(pn);
    }
    //TC2: Test 2: Negative Dosing (Negative dosering)
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void NegativePNDosing_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        service.OpretPN(patient.PatientId, lm.LaegemiddelId, -1, new DateTime(2024, 11, 3), new DateTime(2024, 11, 10));
    }
    
    //TC3
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void PN_NoDatesRegistered_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        var pn = service.OpretPN(patient.PatientId, lm.LaegemiddelId, 3, 
            new DateTime(2024, 11, 01), new DateTime(2024, 11, 10));

        // No dates are registered using givDosis.
        if (!pn.dates.Any())
        {
            throw new InvalidOperationException("Mindst én dato skal registreres.");
        }
    }

    //TC4
    [TestMethod]
    public void PN_MultipleDosesOnSameDay_ShouldPass()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Create the PN ordination
        var pn = service.OpretPN(patient.PatientId, lm.LaegemiddelId, 3, 
            new DateTime(2024, 11, 01), new DateTime(2024, 11, 10));

        // Act
      //  pn.givDosis(new Dato(new DateTime(2024, 11, 03)));
       // pn.givDosis(new Dato(new DateTime(2024, 11, 03))); // Same day

        // Assert
        Assert.AreEqual(2, pn.dates.Count(d => d.dato.Date == new DateTime(2024, 11, 03)));
        Assert.AreEqual(2 * 3, pn.samletDosis()); // Total dose: 6 (2 doses of 3 units each)
    }

//Dagli fast

//TC1
    [TestMethod]
    public void ValidDagligFast_ShouldPass()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        var dagligFast = service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 1, 1, 0, new DateTime(2024, 11, 1), new DateTime(2024, 11, 10));

        // Assert
        Assert.IsNotNull(dagligFast);
    }
//TC2
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ExceedMaximumDoses_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 2, 2, 2, new DateTime(2024, 11, 1), new DateTime(2024, 11, 10));
    }
//TC3
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void NegativeDose_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            -1, 1, 1, 1, new DateTime(2024, 11, 1), new DateTime(2024, 11, 10));
    }
    //TC4
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void OpretDagligFast_MissingDosage_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            0, 0, 0, 0, // No doses provided
            new DateTime(2024, 11, 20), 
            new DateTime(2024, 11, 25));
    }
//Daglige skæv
//TC1
    [TestMethod]
    public void OpretDagligSkaev_ValidTimes_ShouldPass()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        var doser = new Dosis[]
        {
            new Dosis(new DateTime(2024, 11, 20, 9, 0, 0), 2),
            new Dosis(new DateTime(2024, 11, 20, 12, 0, 0), 1),
            new Dosis(new DateTime(2024, 11, 20, 18, 0, 0), 3)
        };

        // Act
        var dagligSkaev = service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser,
            new DateTime(2024, 11, 20), new DateTime(2024, 11, 25));

        // Assert
        Assert.IsNotNull(dagligSkaev);
        Assert.AreEqual(3, dagligSkaev.doser.Count);
        Assert.AreEqual(6, dagligSkaev.samletDosis());
    }
//TC2
    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void OpretDagligSkaev_InvalidTimeFormat_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        var doser = new Dosis[]
        {
            new Dosis(new DateTime(2024, 11, 20, 9, 60, 0), 2) // Invalid time format
        };

        // Act
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser,
            new DateTime(2024, 11, 20), new DateTime(2024, 11, 25));
    }
//TC3
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void OpretDagligSkaev_NegativeDose_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        var doser = new Dosis[]
        {
            new Dosis(new DateTime(2024, 11, 20, 9, 0, 0), -1) // Negative dose
        };

        // Act
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser,
            new DateTime(2024, 11, 20), new DateTime(2024, 11, 25));
    }

    
//Opret ordination use case testing

/*
//TC1
    [TestMethod]
    public void FindPatient_ValidCPR_ShouldPass()
    {
        // Arrange
        var patient = service.GetPatienter().First(p => p.CPR == "123456-7890");

        // Assert
        Assert.IsNotNull(patient);
        Assert.AreEqual("123456-7890", patient.CPR);
    }
*/
//TC2
    [TestMethod]
    public void CreateDagligFastOrdination_ValidData_ShouldPass()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        var dagligFast = service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 1, 1, 0, new DateTime(2024, 11, 20), new DateTime(2024, 11, 25));

        // Assert
        Assert.IsNotNull(dagligFast);
        Assert.AreEqual(2, dagligFast.MorgenDosis.antal);
        Assert.AreEqual(1, dagligFast.MiddagDosis.antal);
        Assert.AreEqual(1, dagligFast.AftenDosis.antal);
        Assert.AreEqual(0, dagligFast.NatDosis.antal);
    }
//TC3
    [TestMethod]
    public void CreatePNOrdination_ValidData_ShouldPass()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        var pn = service.OpretPN(patient.PatientId, lm.LaegemiddelId, 2, 
            new DateTime(2024, 11, 20), new DateTime(2024, 11, 25));

        // Assert
        Assert.IsNotNull(pn);
        Assert.AreEqual(2, pn.antalEnheder);
    }
//TC4
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CreateOrdination_InvalidDates_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // Act
        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 1, 1, 0, new DateTime(2024, 11, 26), new DateTime(2024, 11, 20));
    }

//TC5
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void OpretDagligSkaev_ExceedRecommendedDose_ShouldThrowException()
    {
        // Arrange
        var patient = service.GetPatienter().First();
        var lm = service.GetLaegemidler().First();

        // The dose exceeds the recommended daily dose
        var doser = new Dosis[]
        {
            new Dosis(new DateTime(2024, 11, 20, 8, 0, 0), 150) // Replacing CreateTimeOnly
        };

        // Act
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser, 
            new DateTime(2024, 11, 20), 
            new DateTime(2024, 11, 25));
    }



}