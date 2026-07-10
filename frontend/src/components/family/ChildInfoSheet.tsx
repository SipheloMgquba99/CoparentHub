import { useState, type FC, type FormEvent, type CSSProperties } from "react";
import type { Child } from "../../types";
import * as api from "../../api";
import { Ico, Icons } from "../icons";
import { Spinner } from "../ui";

interface ChildInfoSheetProps {
  familyId: string;
  child: Child;
  onClose: () => void;
  onSaved: () => void;
}

const sectionStyle: CSSProperties = {
  fontSize: 11.5, fontWeight: 600, color: "var(--text-2)",
  letterSpacing: ".5px", textTransform: "uppercase",
  margin: "20px 0 10px", paddingTop: 4, borderTop: "1px solid var(--border)",
};

const ChildInfoSheet: FC<ChildInfoSheetProps> = ({ familyId, child, onClose, onSaved }) => {
  const [allergies, setAllergies] = useState(child.allergies ?? "");
  const [medications, setMedications] = useState(child.medications ?? "");
  const [medicalNotes, setMedicalNotes] = useState(child.medicalNotes ?? "");
  const [doctorName, setDoctorName] = useState(child.doctorName ?? "");
  const [doctorPhone, setDoctorPhone] = useState(child.doctorPhone ?? "");
  const [schoolName, setSchoolName] = useState(child.schoolName ?? "");
  const [schoolContact, setSchoolContact] = useState(child.schoolContact ?? "");
  const [clothingSize, setClothingSize] = useState(child.clothingSize ?? "");
  const [shoeSize, setShoeSize] = useState(child.shoeSize ?? "");
  const [emergencyContactName, setEmergencyContactName] = useState(child.emergencyContactName ?? "");
  const [emergencyContactPhone, setEmergencyContactPhone] = useState(child.emergencyContactPhone ?? "");
  const [err, setErr] = useState("");
  const [busy, setBusy] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErr(""); setBusy(true);
    try {
      await api.updateChildInfo(familyId, child.id, {
        allergies: allergies.trim() || null,
        medications: medications.trim() || null,
        medicalNotes: medicalNotes.trim() || null,
        doctorName: doctorName.trim() || null,
        doctorPhone: doctorPhone.trim() || null,
        schoolName: schoolName.trim() || null,
        schoolContact: schoolContact.trim() || null,
        clothingSize: clothingSize.trim() || null,
        shoeSize: shoeSize.trim() || null,
        emergencyContactName: emergencyContactName.trim() || null,
        emergencyContactPhone: emergencyContactPhone.trim() || null,
      });
      onSaved();
      onClose();
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Failed to save details.");
    }
    setBusy(false);
  };

  return (
    <div className="ov" onClick={e => e.target === e.currentTarget && !busy && onClose()}>
      <div className="sh">
        <div className="shdrag" />
        <div className="shhead">
          <div className="shtitle">{child.name}'s Details</div>
          <button type="button" className="shclose" onClick={onClose} aria-label="Close">
            <Ico d={Icons.x} size={15} />
          </button>
        </div>
        {err && <div className="err">{err}</div>}
        <form onSubmit={handleSubmit}>
          <div style={sectionStyle}>Medical</div>
          <div className="f">
            <label>Allergies</label>
            <input value={allergies} onChange={e => setAllergies(e.target.value)} placeholder="e.g. Peanuts, penicillin" maxLength={500} />
          </div>
          <div className="f">
            <label>Medications</label>
            <input value={medications} onChange={e => setMedications(e.target.value)} placeholder="Current medications" maxLength={500} />
          </div>
          <div className="f">
            <label>Medical Notes</label>
            <input value={medicalNotes} onChange={e => setMedicalNotes(e.target.value)} placeholder="Conditions, special care instructions" maxLength={500} />
          </div>
          <div className="frow">
            <div className="f">
              <label>Doctor Name</label>
              <input value={doctorName} onChange={e => setDoctorName(e.target.value)} placeholder="Dr. Smith" maxLength={150} />
            </div>
            <div className="f">
              <label>Doctor Phone</label>
              <input value={doctorPhone} onChange={e => setDoctorPhone(e.target.value)} placeholder="Phone number" maxLength={30} />
            </div>
          </div>

          <div style={sectionStyle}>School</div>
          <div className="f">
            <label>School Name</label>
            <input value={schoolName} onChange={e => setSchoolName(e.target.value)} placeholder="School name" maxLength={150} />
          </div>
          <div className="f">
            <label>School Contact</label>
            <input value={schoolContact} onChange={e => setSchoolContact(e.target.value)} placeholder="Teacher or office contact" maxLength={150} />
          </div>

          <div style={sectionStyle}>Sizes</div>
          <div className="frow">
            <div className="f">
              <label>Clothing Size</label>
              <input value={clothingSize} onChange={e => setClothingSize(e.target.value)} placeholder="e.g. 6T" maxLength={30} />
            </div>
            <div className="f">
              <label>Shoe Size</label>
              <input value={shoeSize} onChange={e => setShoeSize(e.target.value)} placeholder="e.g. 12" maxLength={30} />
            </div>
          </div>

          <div style={sectionStyle}>Emergency Contact</div>
          <div className="frow">
            <div className="f">
              <label>Name</label>
              <input value={emergencyContactName} onChange={e => setEmergencyContactName(e.target.value)} placeholder="Contact name" maxLength={150} />
            </div>
            <div className="f">
              <label>Phone</label>
              <input value={emergencyContactPhone} onChange={e => setEmergencyContactPhone(e.target.value)} placeholder="Phone number" maxLength={30} />
            </div>
          </div>

          <button className="btn btn-p" type="submit" disabled={busy}>
            {busy ? <Spinner /> : "Save Details"}
          </button>
        </form>
      </div>
    </div>
  );
};

export default ChildInfoSheet;
