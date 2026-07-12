export interface CustodySchedule {
  id: string;
  startDate: string;
  cycleLengthDays: number;
  dayPattern: string;
  parentAUserId: string;
  parentAName: string;
  parentBUserId: string;
  parentBName: string;
}

export interface CustodyDay {
  date: string;
  dayName: string;
  parentUserId: string;
  parentName: string;
}

export interface CustodyRange {
  from: string;
  to: string;
  days: CustodyDay[];
}

export interface CreateCustodyScheduleRequest {
  startDate: string;
  cycleLengthDays: number;
  dayPattern: string;
  parentAUserId: string;
  parentBUserId: string;
}
