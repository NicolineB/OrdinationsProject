using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using shared.Model;
using static shared.Util;
using Data;
using System;

namespace Service;

public class DataService
{
    private OrdinationContext db { get; }

    public DataService(OrdinationContext db) {
        this.db = db;
    } 

    /// <summary>
    /// Seeder noget nyt data i databasen, hvis det er nødvendigt.
    /// </summary>
    public void SeedData() {

        // Patients
        Patient[] patients = new Patient[7];
        patients[0] = db.Patienter.FirstOrDefault()!;

        if (patients[0] == null)
        {
            patients[0] = new Patient("121256-0512", "Jane Jensen", 63.4);
            patients[1] = new Patient("070985-1153", "Finn Madsen", 83.2);
            patients[2] = new Patient("050972-1233", "Hans Jørgensen", 89.4);
            patients[3] = new Patient("011064-1522", "Ulla Nielsen", 59.9);
            patients[4] = new Patient("123456-1234", "Ib Hansen", 87.7);

            patients[5] = new Patient("123457-4321", "Sofie Jørgensen", 20.1);
            patients[6] = new Patient("123459-4301", "JØrgen Jørgensen", 150.5);

            db.Patienter.Add(patients[0]);
            db.Patienter.Add(patients[1]);
            db.Patienter.Add(patients[2]);
            db.Patienter.Add(patients[3]);
            db.Patienter.Add(patients[4]);
            db.Patienter.Add(patients[5]);
            db.Patienter.Add(patients[6]);
            db.SaveChanges();
        }

        Laegemiddel[] laegemiddler = new Laegemiddel[5];
        laegemiddler[0] = db.Laegemiddler.FirstOrDefault()!;
        if (laegemiddler[0] == null)
        {
            laegemiddler[0] = new Laegemiddel("Acetylsalicylsyre", 0.1, 0.15, 0.16, "Styk");
            laegemiddler[1] = new Laegemiddel("Paracetamol", 1, 1.5, 2, "Ml");
            laegemiddler[2] = new Laegemiddel("Fucidin", 0.025, 0.025, 0.025, "Styk");
            laegemiddler[3] = new Laegemiddel("Methotrexat", 0.01, 0.015, 0.02, "Styk");
            laegemiddler[4] = new Laegemiddel("Prednisolon", 0.1, 0.15, 0.2, "Styk");

            db.Laegemiddler.Add(laegemiddler[0]);
            db.Laegemiddler.Add(laegemiddler[1]);
            db.Laegemiddler.Add(laegemiddler[2]);
            db.Laegemiddler.Add(laegemiddler[3]);
            db.Laegemiddler.Add(laegemiddler[4]);

            db.SaveChanges();
        }

        Ordination[] ordinationer = new Ordination[7];
        ordinationer[0] = db.Ordinationer.FirstOrDefault()!;
        if (ordinationer[0] == null) {
            Laegemiddel[] lm = db.Laegemiddler.ToArray();
            Patient[] p = db.Patienter.ToArray();

            ordinationer[0] = new PN(new DateTime(2021, 1, 1), new DateTime(2021, 1, 12), 123, lm[1]);    
            ordinationer[1] = new PN(new DateTime(2021, 2, 12), new DateTime(2021, 2, 14), 3, lm[0]);    
            ordinationer[2] = new PN(new DateTime(2021, 1, 20), new DateTime(2021, 1, 25), 5, lm[2]);    
            ordinationer[3] = new PN(new DateTime(2021, 1, 1), new DateTime(2021, 1, 12), 123, lm[1]);
            ordinationer[4] = new DagligFast(new DateTime(2021, 1, 10), new DateTime(2024, 1, 12), lm[1], 2, 0, 1, 0);
            ordinationer[5] = new DagligSkæv(new DateTime(2021, 1, 23), new DateTime(2021, 1, 24), lm[2]);
            ordinationer[6] = new DagligFast(new DateTime(2022, 3, 16), new DateTime(2022, 3, 30), lm[4], -1, 1, 2, 0);

            ((DagligSkæv) ordinationer[5]).doser = new Dosis[] { 
                new Dosis(CreateTimeOnly(12, 0, 0), 0.5),
                new Dosis(CreateTimeOnly(12, 40, 0), 1),
                new Dosis(CreateTimeOnly(16, 0, 0), 2.5),
                new Dosis(CreateTimeOnly(18, 45, 0), 3)        
            }.ToList();
            

            db.Ordinationer.Add(ordinationer[0]);
            db.Ordinationer.Add(ordinationer[1]);
            db.Ordinationer.Add(ordinationer[2]);
            db.Ordinationer.Add(ordinationer[3]);
            db.Ordinationer.Add(ordinationer[4]);
            db.Ordinationer.Add(ordinationer[5]);
            db.Ordinationer.Add(ordinationer[6]);

            db.SaveChanges();

            p[0].ordinationer.Add(ordinationer[0]);
            p[0].ordinationer.Add(ordinationer[1]);
            p[2].ordinationer.Add(ordinationer[2]);
            p[3].ordinationer.Add(ordinationer[3]);
            p[1].ordinationer.Add(ordinationer[4]);
            p[1].ordinationer.Add(ordinationer[5]);
            p[1].ordinationer.Add(ordinationer[6]);

            db.SaveChanges();
        }
    }

    
    public List<PN> GetPNs() {
        return db.PNs.Include(o => o.laegemiddel).Include(o => o.dates).ToList();
    }

    public List<DagligFast> GetDagligFaste() {
        return db.DagligFaste
            .Include(o => o.laegemiddel)
            .Include(o => o.MorgenDosis)
            .Include(o => o.MiddagDosis)
            .Include(o => o.AftenDosis)            
            .Include(o => o.NatDosis)            
            .ToList();
    }

    public List<DagligSkæv> GetDagligSkæve() {
        return db.DagligSkæve
            .Include(o => o.laegemiddel)
            .Include(o => o.doser)
            .ToList();
    }

    public List<Patient> GetPatienter() {
        return db.Patienter.Include(p => p.ordinationer).ToList();
    }

    public List<Laegemiddel> GetLaegemidler() {
        return db.Laegemiddler.ToList();
    }

    public PN OpretPN(int patientId, int laegemiddelId, double antal, DateTime startDato, DateTime slutDato) {
        // TODO: Implement!
        // her finder det specifikke laegemiddel udfra laegmiddelId og specifikke patient og deres id
        Laegemiddel l = db.Laegemiddler.Find(laegemiddelId);
        Patient p = db.Patienter.FirstOrDefault(p => p.PatientId == patientId);


        PN d = new PN(startDato, slutDato, antal, l);

        //Så længe patiener ikke null og antalenheder ikke er større end Getanbefaletdosisprdøgn og antalenheder ikke er mindre end 1
        if (p != null && d.antalEnheder <= GetAnbefaletDosisPerDøgn(patientId, laegemiddelId) && d.antalEnheder >= 1)
        {
            p.ordinationer.Add(d);
            db.SaveChanges();
            return d;
        }
        
        return null!;
    }

    public DagligFast OpretDagligFast(int patientId, int laegemiddelId, 
        double antalMorgen, double antalMiddag, double antalAften, double antalNat, 
        DateTime startDato, DateTime slutDato) {

        // TODO: Implement!
        // her finder det specifikke laegemiddel udfra laegmiddelId og specifikke patient og deres id
        Laegemiddel l = db.Laegemiddler.Find(laegemiddelId);
        Patient patient = db.Patienter.FirstOrDefault(p => p.PatientId == patientId);

        DagligFast d = new DagligFast(startDato, slutDato, l, antalMorgen, antalMiddag, antalAften, antalNat);

        // Så længe patienten ikke null og Døgndosis ikke er større end anbefaletdosis og døgndosis ikke er negativ
        if (patient != null && (d.MorgenDosis.antal + d.MiddagDosis.antal + d.AftenDosis.antal + d.NatDosis.antal) <= GetAnbefaletDosisPerDøgn(patientId, laegemiddelId)
            && d.MorgenDosis.antal >= 0 && d.MiddagDosis.antal >= 0 && d.AftenDosis.antal >= 0 && d.NatDosis.antal >= 0)
        {
            patient.ordinationer.Add(d);
            db.SaveChanges();
            return d;
        }
        
        return null!;
    }

    public DagligSkæv OpretDagligSkaev(int patientId, int laegemiddelId, Dosis[] doser, DateTime startDato, DateTime slutDato) {
        // TODO: Implement!
        // her finder det specifikke laegemiddel udfra laegmiddelId og specifikke patient og deres id
        Laegemiddel l = db.Laegemiddler.Find(laegemiddelId);
        Patient patient = db.Patienter.FirstOrDefault(p => p.PatientId == patientId);

        DagligSkæv d = new DagligSkæv(startDato, slutDato, l, doser);

        // Så længe patienten ikke null og Døgndosis ikke er større end anbefaletdosis og dosisantal ikke er mindre end 1
        if (patient != null && doser.Sum(x => x.antal) <= GetAnbefaletDosisPerDøgn(patientId, laegemiddelId) && !doser.Any(x => x.antal < 1))
        {
            patient.ordinationer.Add(d);
            db.SaveChanges();
            return d;
        }

        return null!;
        
        
    }

    public string AnvendOrdination(int id, Dato dato) {
        // TODO: Implement!
        //her finder vi den specifikke pn udfra ordinationsId
        PN pn = db.PNs.FirstOrDefault(p => p.OrdinationId == id);

        //Hvis Dosis er inde if gyldighedsprioden så returner den anvendt
        if (pn.givDosis(dato))
        {
            db.SaveChanges();
            return "anvendt";
        }

        return "ikke anvendt";
    }

    /// <summary>
    /// Den anbefalede dosis for den pågældende patient, per døgn, hvor der skal tages hensyn til
	/// patientens vægt. Enheden afhænger af lægemidlet. Patient og lægemiddel må ikke være null.
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="laegemiddel"></param>
    /// <returns></returns>
	public double GetAnbefaletDosisPerDøgn(int patientId, int laegemiddelId) {
        // TODO: Implement!
        // her finder det specifikke laegemiddel udfra laegmiddelId og specifikke patient og deres id
        Patient p = db.Patienter.FirstOrDefault(p => p.PatientId == patientId);
        Laegemiddel l = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId);

        //Her udregner vi anbefaletdosisprdøgn, og tager udgangspunkt i patientensvægt og laegemiddel faktor for den specifikke vægtklasse
        if (p.vaegt < 25)
        {
            return l.enhedPrKgPrDoegnLet * p.vaegt;
        }

        if (p.vaegt > 120)
        {
            return l.enhedPrKgPrDoegnTung * p.vaegt;
        }
        
        return l.enhedPrKgPrDoegnNormal * p.vaegt;
	}
    
}