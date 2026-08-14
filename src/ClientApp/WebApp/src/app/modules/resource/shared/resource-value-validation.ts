import {
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { ResValueType } from 'src/app/services/admin/models/entity/res-value-type.model';

const resourceValueFormatError = 'resourceValueFormat';
const numberPattern = /^[+-]?(?:\d+(?:\.\d*)?|\.\d+)$/;
const datePattern = /^(\d{4})-(\d{2})-(\d{2})$/;
const ipv4Pattern = /^\d{1,3}(?:\.\d{1,3}){3}$/;
const ipv6PartPattern = /^[0-9a-f]{1,4}$/i;

export function resourceValueValidator(valueType: ResValueType): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = String(control.value ?? '');
    if (value.length === 0 || isValidResourceValue(value, valueType)) {
      return null;
    }
    return { [resourceValueFormatError]: true };
  };
}

function isValidResourceValue(value: string, valueType: ResValueType): boolean {
  const trimmedValue = value.trim();
  switch (valueType) {
    case ResValueType.String:
      return true;
    case ResValueType.Number:
      return numberPattern.test(trimmedValue);
    case ResValueType.Boolean:
      return /^(true|false)$/i.test(trimmedValue);
    case ResValueType.Date:
      return isValidDate(trimmedValue);
    case ResValueType.Uri:
      return isAbsoluteUri(trimmedValue);
    case ResValueType.IPAddress:
      return isIpAddress(trimmedValue);
    default:
      return false;
  }
}

function isValidDate(value: string): boolean {
  const match = datePattern.exec(value);
  if (!match) return false;

  const year = Number(match[1]);
  const month = Number(match[2]);
  const day = Number(match[3]);
  const date = new Date(Date.UTC(year, month - 1, day));
  return date.getUTCFullYear() === year
    && date.getUTCMonth() === month - 1
    && date.getUTCDate() === day;
}

function isAbsoluteUri(value: string): boolean {
  try {
    return new URL(value).protocol.length > 0;
  } catch {
    return false;
  }
}

function isIpAddress(value: string): boolean {
  return value.includes(':') ? isIpv6Address(value) : isIpv4Address(value);
}

function isIpv4Address(value: string): boolean {
  if (!ipv4Pattern.test(value)) return false;
  return value.split('.').every((part) => Number(part) <= 255);
}

function isIpv6Address(value: string): boolean {
  if (value.length === 0 || value.includes('%')) return false;

  const sections = value.split('::');
  if (sections.length > 2) return false;

  const leftUnits = countIpv6Units(sections[0]);
  const rightUnits = sections.length === 2 ? countIpv6Units(sections[1]) : 0;
  if (leftUnits === null || rightUnits === null) return false;

  return sections.length === 2
    ? leftUnits + rightUnits < 8
    : leftUnits === 8;
}

function countIpv6Units(value: string): number | null {
  if (value.length === 0) return 0;

  const parts = value.split(':');
  let units = 0;
  for (const [index, part] of parts.entries()) {
    if (part.includes('.')) {
      if (index !== parts.length - 1 || !isIpv4Address(part)) return null;
      units += 2;
    } else if (ipv6PartPattern.test(part)) {
      units++;
    } else {
      return null;
    }
  }
  return units;
}
