// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

// Based on https://github.com/mnarimani/RTLTMPro

// MIT License

// Copyright (c) 2018 Mohamad Narimani @sorencoder

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;

namespace Naninovel.ArabicSupport
{
	public enum TashkeelCharacters
	{
		Fathan = 0x064B,
		Dammatan = 0x064C,
		Kasratan = 0x064D,
		Fatha = 0x064E,
		Damma = 0x064F,
		Kasra = 0x0650,
		Shadda = 0x0651,
		Sukun = 0x0652,
		MaddahAbove = 0x0653,
		SuperscriptAlef = 0x670,
		ShaddaWithDammatanIsolatedForm = 0xFC5E,
		ShaddaWithKasratanIsolatedForm = 0xFC5F,
		ShaddaWithFathaIsolatedForm = 0xFC60,
		ShaddaWithDammaIsolatedForm = 0xFC61,
		ShaddaWithKasraIsolatedForm = 0xFC62,
		ShaddaWithSuperscriptAlefIsolatedForm = 0xFC63
	}

	public enum EnglishNumbers
	{
		Zero = 0x0030,
		One = 0x0031,
		Two = 0x0032,
		Three = 0x0033,
		Four = 0x0034,
		Five = 0x0035,
		Six = 0x0036,
		Seven = 0x0037,
		Eight = 0x0038,
		Nine = 0x0039
	}

	public enum FarsiNumbers
	{
		Zero = 0x6F0,
		One = 0x6F1,
		Two = 0x6F2,
		Three = 0x6F3,
		Four = 0x6F4,
		Five = 0x6F5,
		Six = 0x6F6,
		Seven = 0x6F7,
		Eight = 0x6F8,
		Nine = 0x6F9
	}

	public enum GeneralLetters
	{
		Hamza = 0x0621,
		Alef = 0x0627,
		AlefHamza = 0x0623,
		WawHamza = 0x0624,
		AlefMaksoor = 0x0625,
		AlefMaksura = 0x649,
		HamzaNabera = 0x0626,
		Ba = 0x0628,
		Ta = 0x062A,
		Tha2 = 0x062B,
		Jeem = 0x062C,
		H7aa = 0x062D,
		Khaa2 = 0x062E,
		Dal = 0x062F,
		Thal = 0x0630,
		Ra2 = 0x0631,
		Zeen = 0x0632,
		Seen = 0x0633,
		Sheen = 0x0634,
		S9a = 0x0635,
		Dha = 0x0636,
		T6a = 0x0637,
		T6ha = 0x0638,
		Ain = 0x0639,
		Gain = 0x063A,
		Fa = 0x0641,
		Gaf = 0x0642,
		Kaf = 0x0643,
		Lam = 0x0644,
		Meem = 0x0645,
		Noon = 0x0646,
		Ha = 0x0647,
		Waw = 0x0648,
		Ya = 0x064A,
		AlefMad = 0x0622,
		TaMarboota = 0x0629,
		PersianYa = 0x6CC,
		PersianPe = 0x067E,
		PersianChe = 0x0686,
		PersianZe = 0x0698,
		PersianGaf = 0x06AF,
		PersianGaf2 = 0x06A9,
		ArabicTatweel = 0x640,
		ZeroWidthNoJoiner = 0x200C
	}

	public enum HinduNumbers
	{
		Zero = 0x0660,
		One = 0x0661,
		Two = 0x0662,
		Three = 0x0663,
		Four = 0x0664,
		Five = 0x0665,
		Six = 0x0666,
		Seven = 0x0667,
		Eight = 0x0668,
		Nine = 0x0669
	}

	internal enum IsolatedLetters
	{
		Hamza = 0xFE80,
		Alef = 0xFE8D,
		AlefHamza = 0xFE83,
		WawHamza = 0xFE85,
		AlefMaksoor = 0xFE87,
		AlefMaksura = 0xFEEF,
		HamzaNabera = 0xFE89,
		Ba = 0xFE8F,
		Ta = 0xFE95,
		Tha2 = 0xFE99,
		Jeem = 0xFE9D,
		H7aa = 0xFEA1,
		Khaa2 = 0xFEA5,
		Dal = 0xFEA9,
		Thal = 0xFEAB,
		Ra2 = 0xFEAD,
		Zeen = 0xFEAF,
		Seen = 0xFEB1,
		Sheen = 0xFEB5,
		S9a = 0xFEB9,
		Dha = 0xFEBD,
		T6a = 0xFEC1,
		T6ha = 0xFEC5,
		Ain = 0xFEC9,
		Gain = 0xFECD,
		Fa = 0xFED1,
		Gaf = 0xFED5,
		Kaf = 0xFED9,
		Lam = 0xFEDD,
		Meem = 0xFEE1,
		Noon = 0xFEE5,
		Ha = 0xFEE9,
		Waw = 0xFEED,
		Ya = 0xFEF1,
		AlefMad = 0xFE81,
		TaMarboota = 0xFE93,
		PersianYa = 0xFBFC,
		PersianPe = 0xFB56,
		PersianChe = 0xFB7A,
		PersianZe = 0xFB8A,
		PersianGaf = 0xFB92,
		PersianGaf2 = 0xFB8E

	}

	public static class TextUtils
	{
		// Every English character is between these two
		private const char UpperCaseA = (char)0x41;
		private const char LowerCaseZ = (char)0x7A;

		public static bool IsNumber (char ch, bool preserverNumbers, bool farsi)
		{
			if (preserverNumbers)
				return IsEnglishNumber(ch);

			if (farsi)
				return IsFarsiNumber(ch);

			return IsHinduNumber(ch);
		}

		public static bool IsEnglishNumber (char ch)
		{
			return ch >= (char)EnglishNumbers.Zero && ch <= (char)EnglishNumbers.Nine;
		}

		public static bool IsFarsiNumber (char ch)
		{
			return ch >= (char)FarsiNumbers.Zero && ch <= (char)FarsiNumbers.Nine;
		}

		public static bool IsHinduNumber (char ch)
		{
			return ch >= (char)HinduNumbers.Zero && ch <= (char)HinduNumbers.Nine;
		}

		public static bool IsEnglishLetter (char ch)
		{
			return ch >= UpperCaseA && ch <= LowerCaseZ;
		}


		/// <summary>
		///     Checks if the character is supported RTL character.
		/// </summary>
		/// <param name="ch">Character to check</param>
		/// <returns><see langword="true" /> if character is supported. otherwise <see langword="false" /></returns>
		public static bool IsRTLCharacter (char ch)
		{
			/*
             * Other shapes of each letter comes right after the isolated form.
             * That's why we add 3 to the isolated letter to cover every shape the letter
             */

			if (ch >= (char)IsolatedLetters.Hamza && ch <= (char)IsolatedLetters.Hamza + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Alef && ch <= (char)IsolatedLetters.Alef + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.AlefHamza &&
				ch <= (char)IsolatedLetters.AlefHamza + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.WawHamza && ch <= (char)IsolatedLetters.WawHamza + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.AlefMaksoor &&
				ch <= (char)IsolatedLetters.AlefMaksoor + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.AlefMaksura &&
				ch <= (char)IsolatedLetters.AlefMaksura + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.HamzaNabera &&
				ch <= (char)IsolatedLetters.HamzaNabera + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Ba && ch <= (char)IsolatedLetters.Ba + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Ta && ch <= (char)IsolatedLetters.Ta + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Tha2 && ch <= (char)IsolatedLetters.Tha2 + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Jeem && ch <= (char)IsolatedLetters.Jeem + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.H7aa && ch <= (char)IsolatedLetters.H7aa + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Khaa2 && ch <= (char)IsolatedLetters.Khaa2 + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Dal && ch <= (char)IsolatedLetters.Dal + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Thal && ch <= (char)IsolatedLetters.Thal + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Ra2 && ch <= (char)IsolatedLetters.Ra2 + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Zeen && ch <= (char)IsolatedLetters.Zeen + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Seen && ch <= (char)IsolatedLetters.Seen + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Sheen && ch <= (char)IsolatedLetters.Sheen + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.S9a && ch <= (char)IsolatedLetters.S9a + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Dha && ch <= (char)IsolatedLetters.Dha + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.T6a && ch <= (char)IsolatedLetters.T6a + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.T6ha && ch <= (char)IsolatedLetters.T6ha + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Ain && ch <= (char)IsolatedLetters.Ain + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Gain && ch <= (char)IsolatedLetters.Gain + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Fa && ch <= (char)IsolatedLetters.Fa + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Gaf && ch <= (char)IsolatedLetters.Gaf + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Kaf && ch <= (char)IsolatedLetters.Kaf + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Lam && ch <= (char)IsolatedLetters.Lam + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Meem && ch <= (char)IsolatedLetters.Meem + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Noon && ch <= (char)IsolatedLetters.Noon + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Ha && ch <= (char)IsolatedLetters.Ha + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Waw && ch <= (char)IsolatedLetters.Waw + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.Ya && ch <= (char)IsolatedLetters.Ya + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.AlefMad && ch <= (char)IsolatedLetters.AlefMad + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.TaMarboota &&
				ch <= (char)IsolatedLetters.TaMarboota + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.PersianPe &&
				ch <= (char)IsolatedLetters.PersianPe + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.PersianYa &&
				ch <= (char)IsolatedLetters.PersianYa + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.PersianChe &&
				ch <= (char)IsolatedLetters.PersianChe + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.PersianZe &&
				ch <= (char)IsolatedLetters.PersianZe + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.PersianGaf &&
				ch <= (char)IsolatedLetters.PersianGaf + 3)
			{
				return true;
			}

			if (ch >= (char)IsolatedLetters.PersianGaf2 &&
				ch <= (char)IsolatedLetters.PersianGaf2 + 3)
			{
				return true;
			}

			// Special Lam Alef
			if (ch == 0xFEF3)
			{
				return true;
			}

			if (ch == 0xFEF5)
			{
				return true;
			}

			if (ch == 0xFEF7)
			{
				return true;
			}

			if (ch == 0xFEF9)
			{
				return true;
			}

			switch (ch)
			{
				case (char)GeneralLetters.Hamza:
				case (char)GeneralLetters.Alef:
				case (char)GeneralLetters.AlefHamza:
				case (char)GeneralLetters.WawHamza:
				case (char)GeneralLetters.AlefMaksoor:
				case (char)GeneralLetters.AlefMaksura:
				case (char)GeneralLetters.HamzaNabera:
				case (char)GeneralLetters.Ba:
				case (char)GeneralLetters.Ta:
				case (char)GeneralLetters.Tha2:
				case (char)GeneralLetters.Jeem:
				case (char)GeneralLetters.H7aa:
				case (char)GeneralLetters.Khaa2:
				case (char)GeneralLetters.Dal:
				case (char)GeneralLetters.Thal:
				case (char)GeneralLetters.Ra2:
				case (char)GeneralLetters.Zeen:
				case (char)GeneralLetters.Seen:
				case (char)GeneralLetters.Sheen:
				case (char)GeneralLetters.S9a:
				case (char)GeneralLetters.Dha:
				case (char)GeneralLetters.T6a:
				case (char)GeneralLetters.T6ha:
				case (char)GeneralLetters.Ain:
				case (char)GeneralLetters.Gain:
				case (char)GeneralLetters.Fa:
				case (char)GeneralLetters.Gaf:
				case (char)GeneralLetters.Kaf:
				case (char)GeneralLetters.Lam:
				case (char)GeneralLetters.Meem:
				case (char)GeneralLetters.Noon:
				case (char)GeneralLetters.Ha:
				case (char)GeneralLetters.Waw:
				case (char)GeneralLetters.Ya:
				case (char)GeneralLetters.AlefMad:
				case (char)GeneralLetters.TaMarboota:
				case (char)GeneralLetters.PersianPe:
				case (char)GeneralLetters.PersianChe:
				case (char)GeneralLetters.PersianZe:
				case (char)GeneralLetters.PersianGaf:
				case (char)GeneralLetters.PersianGaf2:
				case (char)GeneralLetters.PersianYa:
				case (char)GeneralLetters.ArabicTatweel:
				case (char)GeneralLetters.ZeroWidthNoJoiner:
					return true;
			}

			return false;
		}

		/// <summary>
		///     Checks if the input string starts with supported RTL character or not.
		/// </summary>
		/// <returns><see langword="true" /> if input is RTL. otherwise <see langword="false" /></returns>
		public static bool IsRTLInput (string input)
		{
			bool insideTag = false;
			foreach (char character in input)
			{
				switch (character)
				{
					case '<':
						insideTag = true;
						continue;
					case '>':
						insideTag = false;
						continue;

					// Arabic Tashkeel
					case (char)TashkeelCharacters.Fathan:
					case (char)TashkeelCharacters.Dammatan:
					case (char)TashkeelCharacters.Kasratan:
					case (char)TashkeelCharacters.Fatha:
					case (char)TashkeelCharacters.Damma:
					case (char)TashkeelCharacters.Kasra:
					case (char)TashkeelCharacters.Shadda:
					case (char)TashkeelCharacters.Sukun:
					case (char)TashkeelCharacters.MaddahAbove:
						return true;
				}

				if (insideTag)
				{
					continue;
				}

				if (char.IsLetter(character))
				{
					return IsRTLCharacter(character);
				}
			}

			return false;
		}
	}

	public struct TashkeelLocation
	{
		public char Tashkeel { get; set; }
		public int Position { get; set; }

		public TashkeelLocation (TashkeelCharacters tashkeel, int position) : this()
		{
			Tashkeel = (char)tashkeel;
			Position = position;
		}
	}

	public static class TashkeelFixer
	{
		private static readonly List<TashkeelLocation> TashkeelLocations = new List<TashkeelLocation>(100);

		private static readonly string ShaddaDammatan = new string(
			new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.Dammatan });

		private static readonly string ShaddaKasratan = new string(
			new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.Kasratan });

		private static readonly string ShaddaSuperscriptAlef = new string(
			new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.SuperscriptAlef });

		private static readonly string ShaddaFatha = new string(
			new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.Fatha });

		private static readonly string ShaddaDamma = new string(
			new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.Damma });

		private static readonly string ShaddaKasra = new string(
			new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.Kasra });

		private static readonly string ShaddaWithFathaIsolatedForm =
		((char)TashkeelCharacters.ShaddaWithFathaIsolatedForm).ToString();

		private static readonly string ShaddaWithDammaIsolatedForm =
		((char)TashkeelCharacters.ShaddaWithDammaIsolatedForm).ToString();

		private static readonly string ShaddaWithKasraIsolatedForm =
		((char)TashkeelCharacters.ShaddaWithKasraIsolatedForm).ToString();

		private static readonly string ShaddaWithDammatanIsolatedForm =
		((char)TashkeelCharacters.ShaddaWithDammatanIsolatedForm).ToString();

		private static readonly string ShaddaWithKasratanIsolatedForm =
		((char)TashkeelCharacters.ShaddaWithKasratanIsolatedForm).ToString();

		private static readonly string ShaddaWithSuperscriptAlefIsolatedForm =
		((char)TashkeelCharacters.ShaddaWithSuperscriptAlefIsolatedForm).ToString();

		/// <summary>
		///     Removes tashkeel from text.
		/// </summary>
		public static void RemoveTashkeel (FastStringBuilder input)
		{
			for (int i = 0; i < input.Length; i++)
			{
				switch ((TashkeelCharacters)input.Get(i))
				{
					case TashkeelCharacters.Fathan:
						TashkeelLocations.Add(new TashkeelLocation(TashkeelCharacters.Fathan, i));
						break;
					case TashkeelCharacters.Dammatan:
						TashkeelLocations.Add(new TashkeelLocation(TashkeelCharacters.Dammatan, i));
						break;
					case TashkeelCharacters.Kasratan:
						TashkeelLocations.Add(new TashkeelLocation(TashkeelCharacters.Kasratan, i));
						break;
					case TashkeelCharacters.Fatha:
						TashkeelLocations.Add(new TashkeelLocation(TashkeelCharacters.Fatha, i));
						break;
					case TashkeelCharacters.Damma:
						TashkeelLocations.Add(new TashkeelLocation(TashkeelCharacters.Damma, i));
						break;
					case TashkeelCharacters.Kasra:
						TashkeelLocations.Add(new TashkeelLocation(TashkeelCharacters.Kasra, i));
						break;
					case TashkeelCharacters.Shadda:
						TashkeelLocations.Add(new TashkeelLocation(TashkeelCharacters.Shadda, i));
						break;
					case TashkeelCharacters.Sukun:
						TashkeelLocations.Add(new TashkeelLocation(TashkeelCharacters.Sukun, i));
						break;
					case TashkeelCharacters.MaddahAbove:
						TashkeelLocations.Add(
							new TashkeelLocation(TashkeelCharacters.MaddahAbove, i));
						break;
					case TashkeelCharacters.SuperscriptAlef:
						TashkeelLocations.Add(
							new TashkeelLocation(TashkeelCharacters.SuperscriptAlef, i));
						break;
				}
			}


			input.RemoveAll((char)TashkeelCharacters.Fathan);
			input.RemoveAll((char)TashkeelCharacters.Dammatan);
			input.RemoveAll((char)TashkeelCharacters.Kasratan);
			input.RemoveAll((char)TashkeelCharacters.Fatha);
			input.RemoveAll((char)TashkeelCharacters.Damma);
			input.RemoveAll((char)TashkeelCharacters.Kasra);
			input.RemoveAll((char)TashkeelCharacters.Shadda);
			input.RemoveAll((char)TashkeelCharacters.Sukun);
			input.RemoveAll((char)TashkeelCharacters.MaddahAbove);
			input.RemoveAll((char)TashkeelCharacters.ShaddaWithFathaIsolatedForm);
			input.RemoveAll((char)TashkeelCharacters.ShaddaWithDammaIsolatedForm);
			input.RemoveAll((char)TashkeelCharacters.ShaddaWithKasraIsolatedForm);
			input.RemoveAll((char)TashkeelCharacters.SuperscriptAlef);
		}

		/// <summary>
		///     Restores removed tashkeel.
		/// </summary>
		public static void RestoreTashkeel (FastStringBuilder letters)
		{
			int letterWithTashkeelTracker = 0;
			foreach (TashkeelLocation location in TashkeelLocations)
			{
				letters.Insert(location.Position + letterWithTashkeelTracker, location.Tashkeel);
				//letterWithTashkeelTracker++;
			}

			/*
             * Fix of https://github.com/mnarimani/RTLTMPro/issues/13
             * The workaround is to replace Shadda + Another Tashkeel with combined form 
             */
			letters.Replace(ShaddaFatha, ShaddaWithFathaIsolatedForm);

			letters.Replace(ShaddaDamma, ShaddaWithDammaIsolatedForm);

			letters.Replace(ShaddaKasra, ShaddaWithKasraIsolatedForm);

			letters.Replace(ShaddaDammatan, ShaddaWithDammatanIsolatedForm);

			letters.Replace(ShaddaKasratan, ShaddaWithKasratanIsolatedForm);

			letters.Replace(ShaddaSuperscriptAlef, ShaddaWithSuperscriptAlefIsolatedForm);

			TashkeelLocations.Clear();
		}
	}

	public static class RTLSupport
	{
		public const int DefaultBufferSize = 2048;

		private static FastStringBuilder inputBuilder;
		private static FastStringBuilder glyphFixerOutput;

		static RTLSupport ()
		{
			inputBuilder = new FastStringBuilder(DefaultBufferSize);
			glyphFixerOutput = new FastStringBuilder(DefaultBufferSize);
		}

		/// <summary>
		///     Fixes the provided string
		/// </summary>
		/// <param name="input">Text to fix</param>
		/// <param name="output">Fixed text</param>
		/// <param name="fixTextTags"></param>
		/// <param name="preserveNumbers"></param>
		/// <param name="farsi"></param>
		/// <returns>Fixed text</returns>
		public static void FixRTL (
			 string input,
			 FastStringBuilder output,
			 bool farsi = true,
			 bool fixTextTags = true,
			 bool preserveNumbers = false)
		{
			inputBuilder.SetValue(input);

			TashkeelFixer.RemoveTashkeel(inputBuilder);

			// The shape of the letters in shapeFixedLetters is fixed according to their position in word. But the flow of the text is not fixed.
			GlyphFixer.Fix(inputBuilder, glyphFixerOutput, preserveNumbers, farsi);

			//Restore tashkeel to their places.
			TashkeelFixer.RestoreTashkeel(glyphFixerOutput);

			// Fix flow of the text and put the result in FinalLetters field
			LigatureFixer.Fix(glyphFixerOutput, output, farsi, fixTextTags, preserveNumbers);

			if (fixTextTags)
			{
				RichTextFixer.Fix(output);
			}

			inputBuilder.Clear();
		}

	}

	public static class RichTextFixer
	{
		private readonly struct Tag
		{
			public readonly int Start;
			public readonly int End;

			public Tag (int start, int end)
			{
				Start = start;
				End = end;
			}
		}

		private static readonly List<Tag> ClosedTags = new List<Tag>(64);
		private static readonly List<int> ClosedTagsHash = new List<int>(64);

		/// <summary>
		///     Fixes rich text tags in input string and returns the result.
		/// </summary>
		public static void Fix (FastStringBuilder text)
		{
			for (int i = 0; i < text.Length; i++)
			{
				FindTag(text, i, out int tagStart, out int tagEnd, out int tagType, out int hashCode);

				// If we couldn't find a tag, end the process
				if (tagType == 0)
				{
					break;
				}

				switch (tagType)
				{
					case 1: // Opening tag
						{
							Tag closingTag = default;

							// Search and find the closing tag for this
							bool foundClosingTag = false;
							for (int j = ClosedTagsHash.Count - 1; j >= 0; j--)
							{
								if (ClosedTagsHash[j] == hashCode)
								{
									closingTag = ClosedTags[j];
									foundClosingTag = true;
									ClosedTags.RemoveAt(j);
									ClosedTagsHash.RemoveAt(j);
									break;
								}
							}

							if (foundClosingTag)
							{
								// NOTE: order of execution is important here

								int openingTagLength = tagEnd - tagStart;
								int closingTagLength = closingTag.End - closingTag.Start;

								text.Reverse(tagStart, openingTagLength);
								text.Reverse(closingTag.Start, closingTagLength);
							}
							else
							{
								text.Reverse(tagStart, tagEnd - tagStart);
							}

							break;
						}
					case 2: // Closing tag
						{
							ClosedTags.Add(new Tag(tagStart, tagEnd));
							ClosedTagsHash.Add(hashCode);
							break;
						}
					case 3: // Self contained tag
						{
							text.Reverse(tagStart, tagEnd - tagStart);
							break;
						}
				}

				i = tagEnd;
			}
		}

		public static void FindTag (
			FastStringBuilder str,
			int start,
			out int tagStart,
			out int tagEnd,
			out int tagType,
			out int hashCode)
		{
			for (int i = start; i < str.Length;)
			{
				if (str.Get(i) != '<')
				{
					i++;
					continue;
				}

				bool calculateHashCode = true;
				hashCode = 0;
				for (int j = i + 1; j < str.Length; j++)
				{
					char jChar = str.Get(j);

					if (calculateHashCode)
					{
						if (char.IsLetter(jChar))
						{
							unchecked
							{
								if (hashCode == 0)
								{
									hashCode = jChar.GetHashCode();
								}
								else
								{
									hashCode = (hashCode * 397) ^ jChar.GetHashCode();
								}
							}
						}
						else if (hashCode != 0)
						{
							// We have computed the hash code. Now we reached a non letter character. We need to stop
							calculateHashCode = false;
						}
					}

					// Rich text tag cannot contain RTL chars
					if (TextUtils.IsRTLCharacter(jChar))
					{
						break;
					}

					if (jChar == '>')
					{
						// Check if the tag is closing, opening or self contained

						tagStart = i;
						tagEnd = j + 1;

						if (str.Get(j - 1) == '/')
						{
							// This is self contained.
							tagType = 3;
							return;
						}

						if (str.Get(i + 1) == '/')
						{
							// This is closing
							tagType = 2;
							return;
						}

						tagType = 1;
						return;
					}
				}

				i++;
			}

			tagStart = 0;
			tagEnd = 0;
			tagType = 0;
			hashCode = 0;
		}
	}

	public static class LigatureFixer
	{
		private static readonly List<char> LtrTextHolder = new List<char>(512);

		/// <summary>
		///     Fixes the flow of the text.
		/// </summary>
		public static void Fix (FastStringBuilder input, FastStringBuilder output, bool farsi, bool fixTextTags, bool preserveNumbers)
		{
			// Some texts like tags, English words and numbers need to be displayed in their original order.
			// This list keeps the characters that their order should be reserved and streams reserved texts into final letters.
			LtrTextHolder.Clear();
			for (int i = input.Length - 1; i >= 0; i--)
			{
				bool isInMiddle = i > 0 && i < input.Length - 1;
				bool isAtBeginning = i == 0;
				bool isAtEnd = i == input.Length - 1;

				char characterAtThisIndex = input.Get(i);

				char nextCharacter = default;
				if (!isAtEnd)
					nextCharacter = input.Get(i + 1);

				char previousCharacter = default;
				if (!isAtBeginning)
					previousCharacter = input.Get(i - 1);

				if (char.IsPunctuation(characterAtThisIndex) || char.IsSymbol(characterAtThisIndex))
				{
					if (fixTextTags)
					{
						if (characterAtThisIndex == '>')
						{
							// We need to check if it is actually the beginning of a tag.
							bool isValidTag = false;
							// If > is at the end of the text (At beginning of the array), it can't be a tag
							if (isAtEnd == false)
							{
								for (int j = i - 1; j >= 0; j--)
								{
									// Tags do not have space inside
									if (input.Get(j) == ' ')
									{
										break;
									}

									// Tags do not have RTL characters inside
									if (TextUtils.IsRTLCharacter(input.Get(j)))
									{
										break;
									}

									if (input.Get(j) == '<')
									{
										isValidTag = true;
										break;
									}
								}
							}

							if (LtrTextHolder.Count > 0 && isValidTag)
							{
								for (int j = 0; j < LtrTextHolder.Count; j++)
								{
									output.Append(LtrTextHolder[LtrTextHolder.Count - 1 - j]);
								}

								LtrTextHolder.Clear();
							}
						}
					}

					if (characterAtThisIndex == ')')
					{
						if (isInMiddle)
						{
							bool isAfterRTLCharacter = TextUtils.IsRTLCharacter(previousCharacter);
							bool isBeforeRTLCharacter = TextUtils.IsRTLCharacter(nextCharacter);

							if (isAfterRTLCharacter || isBeforeRTLCharacter)
							{
								characterAtThisIndex = '(';
							}
						}
						else if (isAtEnd)
						{
							bool isAfterRTLCharacter = TextUtils.IsRTLCharacter(previousCharacter);
							if (isAfterRTLCharacter)
							{
								characterAtThisIndex = '(';
							}
						}
						else if (isAtBeginning)
						{
							bool isBeforeRTLCharacter = TextUtils.IsRTLCharacter(nextCharacter);
							if (isBeforeRTLCharacter)
							{
								characterAtThisIndex = '(';
							}
						}
					}
					else if (characterAtThisIndex == '(')
					{
						if (isInMiddle)
						{
							bool isAfterRTLCharacter = TextUtils.IsRTLCharacter(previousCharacter);
							bool isBeforeRTLCharacter = TextUtils.IsRTLCharacter(nextCharacter);

							if (isAfterRTLCharacter || isBeforeRTLCharacter)
							{
								characterAtThisIndex = ')';
							}
						}
						else if (isAtEnd)
						{
							bool isAfterRTLCharacter = TextUtils.IsRTLCharacter(previousCharacter);
							if (isAfterRTLCharacter)
							{
								characterAtThisIndex = ')';
							}
						}
						else if (isAtBeginning)
						{
							bool isBeforeRTLCharacter = TextUtils.IsRTLCharacter(nextCharacter);
							if (isBeforeRTLCharacter)
							{
								characterAtThisIndex = ')';
							}
						}
					}
					else if (characterAtThisIndex == '«')
					{
						if (isInMiddle)
						{
							bool isAfterRTLCharacter = TextUtils.IsRTLCharacter(nextCharacter);
							bool isBeforeRTLCharacter = TextUtils.IsRTLCharacter(previousCharacter);

							if (isAfterRTLCharacter || isBeforeRTLCharacter)
							{
								characterAtThisIndex = '»';
							}
						}
						else if (isAtEnd)
						{
							bool isAfterRTLCharacter = TextUtils.IsRTLCharacter(nextCharacter);
							if (isAfterRTLCharacter)
							{
								characterAtThisIndex = '»';
							}
						}
						else if (isAtBeginning)
						{
							bool isBeforeRTLCharacter = TextUtils.IsRTLCharacter(previousCharacter);
							if (isBeforeRTLCharacter)
							{
								characterAtThisIndex = '»';
							}
						}
					}
					else if (characterAtThisIndex == '»')
					{
						if (isInMiddle)
						{
							bool isAfterRTLCharacter = TextUtils.IsRTLCharacter(nextCharacter);
							bool isBeforeRTLCharacter = TextUtils.IsRTLCharacter(previousCharacter);

							if (isAfterRTLCharacter || isBeforeRTLCharacter)
							{
								characterAtThisIndex = '«';
							}
						}
						else if (isAtEnd)
						{
							bool isAfterRTLCharacter = TextUtils.IsRTLCharacter(nextCharacter);
							if (isAfterRTLCharacter)
							{
								characterAtThisIndex = '«';
							}
						}
						else if (isAtBeginning)
						{
							bool isBeforeRTLCharacter = TextUtils.IsRTLCharacter(previousCharacter);
							if (isBeforeRTLCharacter)
							{
								characterAtThisIndex = '«';
							}
						}
					}

					if (isInMiddle)
					{
						bool isAfterRTLCharacter = TextUtils.IsRTLCharacter(previousCharacter);
						bool isBeforeRTLCharacter = TextUtils.IsRTLCharacter(nextCharacter);
						bool isBeforeWhiteSpace = char.IsWhiteSpace(nextCharacter);
						bool isAfterWhiteSpace = char.IsWhiteSpace(previousCharacter);
						bool isUnderline = characterAtThisIndex == '_';
						bool isSpecialPunctuation = characterAtThisIndex == '.' ||
						characterAtThisIndex == '،' ||
						characterAtThisIndex == '؛';

						if (isBeforeRTLCharacter && isAfterRTLCharacter ||
							isAfterWhiteSpace && isSpecialPunctuation ||
							isBeforeWhiteSpace && isAfterRTLCharacter ||
							isBeforeRTLCharacter && isAfterWhiteSpace ||
							(isBeforeRTLCharacter || isAfterRTLCharacter) && isUnderline)
						{
							if (LtrTextHolder.Count > 0)
							{
								for (int j = 0; j < LtrTextHolder.Count; j++)
								{
									output.Append(LtrTextHolder[LtrTextHolder.Count - 1 - j]);
								}

								LtrTextHolder.Clear();
							}

							output.Append(characterAtThisIndex);
						}
						else
						{
							LtrTextHolder.Add(characterAtThisIndex);
						}
					}
					else if (isAtEnd)
					{
						LtrTextHolder.Add(characterAtThisIndex);
					}
					else if (isAtBeginning)
					{
						output.Append(characterAtThisIndex);
					}

					if (fixTextTags)
					{
						if (characterAtThisIndex == '<')
						{
							bool valid = false;

							if (isAtBeginning == false)
							{
								for (int j = i + 1; j < input.Length; j++)
								{
									// Tags do not have space inside
									if (input.Get(j) == ' ')
									{
										break;
									}

									// Tags do not have RTL characters inside
									if (TextUtils.IsRTLCharacter(input.Get(j)))
									{
										break;
									}

									if (input.Get(j) == '>')
									{
										valid = true;
										break;
									}
								}
							}

							if (LtrTextHolder.Count > 0 && valid)
							{
								for (int j = 0; j < LtrTextHolder.Count; j++)
								{
									output.Append(LtrTextHolder[LtrTextHolder.Count - 1 - j]);
								}

								LtrTextHolder.Clear();
							}
						}
					}

					continue;
				}

				if (isInMiddle)
				{
					bool isAfterEnglishChar = TextUtils.IsEnglishLetter(previousCharacter);
					bool isBeforeEnglishChar = TextUtils.IsEnglishLetter(nextCharacter);
					bool isAfterNumber = TextUtils.IsNumber(previousCharacter, preserveNumbers, farsi);
					bool isBeforeNumber = TextUtils.IsNumber(nextCharacter, preserveNumbers, farsi);
					bool isAfterSymbol = char.IsSymbol(previousCharacter);
					bool isBeforeSymbol = char.IsSymbol(nextCharacter);

					// For cases where english words and farsi/arabic are mixed. This allows for using farsi/arabic, english and numbers in one sentence.
					// If the space is between numbers,symbols or English words, keep the order
					if (characterAtThisIndex == ' ' &&
						(isBeforeEnglishChar || isBeforeNumber || isBeforeSymbol) &&
						(isAfterEnglishChar || isAfterNumber || isAfterSymbol))
					{
						LtrTextHolder.Add(characterAtThisIndex);
						continue;
					}
				}

				if (TextUtils.IsEnglishLetter(characterAtThisIndex) ||
					TextUtils.IsNumber(characterAtThisIndex, preserveNumbers, farsi))
				{
					LtrTextHolder.Add(characterAtThisIndex);
					continue;
				}

				if (characterAtThisIndex >= (char)0xD800 &&
					characterAtThisIndex <= (char)0xDBFF ||
					characterAtThisIndex >= (char)0xDC00 && characterAtThisIndex <= (char)0xDFFF)
				{
					LtrTextHolder.Add(characterAtThisIndex);
					continue;
				}

				if (LtrTextHolder.Count > 0)
				{
					for (int j = 0; j < LtrTextHolder.Count; j++)
					{
						output.Append(LtrTextHolder[LtrTextHolder.Count - 1 - j]);
					}

					LtrTextHolder.Clear();
				}

				if (characterAtThisIndex != 0xFFFF &&
					characterAtThisIndex != (int)GeneralLetters.ZeroWidthNoJoiner)
				{
					output.Append(characterAtThisIndex);
				}
			}

			if (LtrTextHolder.Count > 0)
			{
				for (int j = 0; j < LtrTextHolder.Count; j++)
				{
					output.Append(LtrTextHolder[LtrTextHolder.Count - 1 - j]);
				}

				LtrTextHolder.Clear();
			}
		}
	}

	/// <summary>
	///     Sets up and creates the conversion table
	/// </summary>
	public static class GlyphTable
	{
		private static readonly Dictionary<char, char> MapList;

		/// <summary>
		///     Setting up the conversion table
		/// </summary>
		static GlyphTable ()
		{
			//using GetNames instead of GetValues to be able to match enums
			var isolatedValues = Enum.GetNames(typeof(IsolatedLetters));

			MapList = new Dictionary<char, char>(isolatedValues.Length);
			foreach (var value in isolatedValues)
				MapList.Add((char)(int)Enum.Parse(typeof(GeneralLetters), value), (char)(int)Enum.Parse(typeof(IsolatedLetters), value));
		}

		public static char Convert (char toBeConverted)
		{
			return MapList.TryGetValue(toBeConverted, out var convertedValue) ? convertedValue : toBeConverted;
		}
	}

	public class FastStringBuilder
	{
		// Using fields to be as efficient as possible
		public int Length;

		private char[] array;
		private int capacity;

		public FastStringBuilder (int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity));

			this.capacity = capacity;
			array = new char[capacity];
		}

		public FastStringBuilder (string text) : this(text, text.Length)
		{
		}

		public FastStringBuilder (string text, int capacity) : this(capacity)
		{
			SetValue(text);
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char Get (int index)
		{
			return array[index];
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set (int index, char ch)
		{
			array[index] = ch;
		}

		public void SetValue (string text)
		{
			Length = text.Length;
			EnsureCapacity(Length, false);

			for (int i = 0; i < text.Length; i++) array[i] = text[i];
		}

		public void SetValue (FastStringBuilder other)
		{
			EnsureCapacity(other.Length, false);
			Copy(other.array, array);
			Length = other.Length;
		}

		public void Append (char ch)
		{
			if (capacity < Length)
				EnsureCapacity(Length, true);

			array[Length] = ch;
			Length++;
		}

		public void Insert (int pos, FastStringBuilder str, int offset, int count)
		{
			if (str == this) throw new InvalidOperationException("You cannot pass the same string builder to insert");
			if (count == 0) return;

			Length += count;
			EnsureCapacity(Length, true);

			for (int i = Length - count - 1; i >= pos; i--)
			{
				array[i + count] = array[i];
			}

			for (int i = 0; i < count; i++)
			{
				array[pos + i] = str.array[offset + i];
			}
		}

		public void Insert (int pos, FastStringBuilder str)
		{
			Insert(pos, str, 0, str.Length);
		}

		public void Insert (int pos, char ch)
		{
			Length++;
			EnsureCapacity(Length, true);

			for (int i = Length - 2; i >= pos; i--)
				array[i + 1] = array[i];

			array[pos] = ch;
		}

		public void RemoveAll (char character)
		{
			for (int i = Length - 1; i >= 0; i--)
			{
				if (array[i] == character)
					Remove(i, 1);
			}
		}

		public void Remove (int start, int length)
		{
			for (int i = start; i < Length - length; i++)
			{
				array[i] = array[i + length];
			}

			Length -= length;
		}

		public void Reverse (int startIndex, int length)
		{
			for (int i = 0; i < length / 2; i++)
			{
				int firstIndex = startIndex + i;
				int secondIndex = startIndex + length - i - 1;

				char first = array[firstIndex];
				char second = array[secondIndex];

				array[firstIndex] = second;
				array[secondIndex] = first;
			}
		}

		public void Reverse ()
		{
			Reverse(0, Length);
		}

		public void Substring (FastStringBuilder output, int start, int length)
		{
			output.Length = 0;
			for (int i = 0; i < length; i++)
				output.Append(array[start + i]);
		}

		public override string ToString ()
		{
			return new string(array, 0, Length);
		}

		public void Replace (char oldChar, char newChar)
		{
			for (int i = 0; i < Length; i++)
			{
				if (array[i] == oldChar)
					array[i] = newChar;
			}
		}

		public void Replace (string oldStr, string newStr)
		{
			for (int i = 0; i < Length; i++)
			{
				bool match = true;
				for (int j = 0; j < oldStr.Length; j++)
				{
					if (array[i + j] != oldStr[j])
					{
						match = false;
						break;
					}
				}

				if (!match) continue;

				if (oldStr.Length == newStr.Length)
				{
					for (int k = 0; k < oldStr.Length; k++)
					{
						array[i + k] = newStr[k];
					}
				}
				else if (oldStr.Length < newStr.Length)
				{
					// We need to expand capacity
					int diff = newStr.Length - oldStr.Length;
					Length += diff;
					EnsureCapacity(Length, true);

					// Move everything forward by difference of length
					for (int k = Length - diff - 1; k >= i + oldStr.Length; k--)
					{
						array[k + diff] = array[k];
					}

					// Start writing new string
					for (int k = 0; k < newStr.Length; k++)
					{
						array[i + k] = newStr[k];
					}
				}
				else
				{
					// We need to shrink
					int diff = oldStr.Length - newStr.Length;

					// Move everything backwards by diff
					for (int k = i + diff; k < Length - diff; k++)
					{
						array[k] = array[k + diff];
					}

					for (int k = 0; k < newStr.Length; k++)
					{
						array[i + k] = newStr[k];
					}

					Length -= diff;
				}

				i += newStr.Length;
			}
		}

		public void Clear ()
		{
			Length = 0;
		}

		private void EnsureCapacity (int cap, bool keepValues)
		{
			if (capacity >= cap)
				return;

			if (capacity == 0)
				capacity = 1;

			while (capacity < cap)
				capacity *= 2;

			if (keepValues)
			{
				char[] newArray = new char[capacity];
				Copy(array, newArray);
				array = newArray;
			}
			else
			{
				array = new char[capacity];
			}
		}

		private static void Copy (char[] src, char[] dst)
		{
			for (int i = 0; i < src.Length; i++)
				dst[i] = src[i];
		}
	}

	public static class GlyphFixer
	{
		/// <summary>
		///     Fixes the shape of letters based on their position.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="preserveNumbers"></param>
		/// <param name="farsi"></param>
		/// <returns></returns>
		public static void Fix (FastStringBuilder input, FastStringBuilder output, bool preserveNumbers, bool farsi)
		{
			FixYah(input, farsi);

			output.SetValue(input);

			for (int i = 0; i < input.Length; i++)
			{
				bool skipNext = false;
				char iChar = input.Get(i);

				// For special Lam Letter connections.
				if (iChar == (char)GeneralLetters.Lam)
				{
					if (i < input.Length - 1)
					{
						skipNext = HandleSpecialLam(input, output, i);
						if (skipNext)
							iChar = output.Get(i);
					}
				}

				// We don't want to fix tatweel or zwnj character
				if (iChar == (int)GeneralLetters.ArabicTatweel ||
					iChar == (int)GeneralLetters.ZeroWidthNoJoiner)
				{
					continue;
				}

				if (TextUtils.IsRTLCharacter(iChar))
				{
					char converted = GlyphTable.Convert(iChar);

					if (IsMiddleLetter(input, i))
					{
						output.Set(i, (char)(converted + 3));
					}
					else if (IsFinishingLetter(input, i))
					{
						output.Set(i, (char)(converted + 1));
					}
					else if (IsLeadingLetter(input, i))
					{
						output.Set(i, (char)(converted + 2));
					}
				}

				// If this letter as Lam and special Lam-Alef connection was made, We want to skip the Alef
				// (Lam-Alef occupies 1 space)
				if (skipNext)
				{
					i++;
				}
			}

			if (!preserveNumbers)
			{
				FixNumbers(output, farsi);
			}
		}

		/// <summary>
		///     Removes tashkeel. Converts general RTL letters to isolated form. Also fixes Farsi and Arabic ی letter.
		/// </summary>
		/// <param name="text">Input to prepare</param>
		/// <param name="farsi"></param>
		/// <returns>Prepared input in char array</returns>
		public static void FixYah (FastStringBuilder text, bool farsi)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (farsi && text.Get(i) == (int)GeneralLetters.Ya)
				{
					text.Set(i, (char)GeneralLetters.PersianYa);
				}
				else if (farsi == false && text.Get(i) == (int)GeneralLetters.PersianYa)
				{
					text.Set(i, (char)GeneralLetters.Ya);
				}
			}
		}

		/// <summary>
		///     Handles the special Lam-Alef connection in the text.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="i">Index of Lam letter</param>
		/// <returns><see langword="true" /> if special connection has been made.</returns>
		private static bool HandleSpecialLam (FastStringBuilder input, FastStringBuilder output, int i)
		{
			bool isFixed;
			switch (input.Get(i + 1))
			{
				case (char)GeneralLetters.AlefMaksoor:
					output.Set(i, (char)0xFEF7);
					isFixed = true;
					break;
				case (char)GeneralLetters.Alef:
					output.Set(i, (char)0xFEF9);
					isFixed = true;
					break;
				case (char)GeneralLetters.AlefHamza:
					output.Set(i, (char)0xFEF5);
					isFixed = true;
					break;
				case (char)GeneralLetters.AlefMad:
					output.Set(i, (char)0xFEF3);
					isFixed = true;
					break;
				default:
					isFixed = false;
					break;
			}

			if (isFixed)
			{
				output.Set(i + 1, (char)0xFFFF);
			}

			return isFixed;
		}

		/// <summary>
		///     Converts English numbers to Persian or Arabic numbers.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="farsi"></param>
		/// <returns>Converted number</returns>
		public static void FixNumbers (FastStringBuilder text, bool farsi)
		{
			text.Replace((char)EnglishNumbers.Zero, farsi ? (char)FarsiNumbers.Zero : (char)HinduNumbers.Zero);
			text.Replace((char)EnglishNumbers.One, farsi ? (char)FarsiNumbers.One : (char)HinduNumbers.One);
			text.Replace((char)EnglishNumbers.Two, farsi ? (char)FarsiNumbers.Two : (char)HinduNumbers.Two);
			text.Replace((char)EnglishNumbers.Three, farsi ? (char)FarsiNumbers.Three : (char)HinduNumbers.Three);
			text.Replace((char)EnglishNumbers.Four, farsi ? (char)FarsiNumbers.Four : (char)HinduNumbers.Four);
			text.Replace((char)EnglishNumbers.Five, farsi ? (char)FarsiNumbers.Five : (char)HinduNumbers.Five);
			text.Replace((char)EnglishNumbers.Six, farsi ? (char)FarsiNumbers.Six : (char)HinduNumbers.Six);
			text.Replace((char)EnglishNumbers.Seven, farsi ? (char)FarsiNumbers.Seven : (char)HinduNumbers.Seven);
			text.Replace((char)EnglishNumbers.Eight, farsi ? (char)FarsiNumbers.Eight : (char)HinduNumbers.Eight);
			text.Replace((char)EnglishNumbers.Nine, farsi ? (char)FarsiNumbers.Nine : (char)HinduNumbers.Nine);
		}

		/// <summary>
		///     Is the letter at provided index a leading letter?
		/// </summary>
		/// <returns><see langword="true" /> if the letter is a leading letter</returns>
		private static bool IsLeadingLetter (FastStringBuilder letters, int index)
		{
			var currentIndexLetter = letters.Get(index);

			char previousIndexLetter = default;
			if (index != 0)
				previousIndexLetter = letters.Get(index - 1);

			char nextIndexLetter = default;
			if (index < letters.Length - 1)
				nextIndexLetter = letters.Get(index + 1);

			bool isPreviousLetterNonConnectable = index == 0 ||
			!TextUtils.IsRTLCharacter(previousIndexLetter) ||
			previousIndexLetter == (int)GeneralLetters.Alef ||
			previousIndexLetter == (int)GeneralLetters.Dal ||
			previousIndexLetter == (int)GeneralLetters.Thal ||
			previousIndexLetter == (int)GeneralLetters.Ra2 ||
			previousIndexLetter == (int)GeneralLetters.Zeen ||
			previousIndexLetter == (int)GeneralLetters.PersianZe ||
			previousIndexLetter == (int)GeneralLetters.Waw ||
			previousIndexLetter == (int)GeneralLetters.AlefMad ||
			previousIndexLetter ==
			(int)GeneralLetters.AlefHamza ||
			previousIndexLetter ==
			(int)GeneralLetters.Hamza ||
			previousIndexLetter ==
			(int)GeneralLetters.AlefMaksoor ||
			previousIndexLetter ==
			(int)GeneralLetters.ZeroWidthNoJoiner ||
			previousIndexLetter ==
			(int)GeneralLetters.WawHamza ||
			previousIndexLetter ==
			(int)IsolatedLetters.Alef ||
			previousIndexLetter == (int)IsolatedLetters.Dal ||
			previousIndexLetter ==
			(int)IsolatedLetters.Thal ||
			previousIndexLetter == (int)IsolatedLetters.Ra2 ||
			previousIndexLetter ==
			(int)IsolatedLetters.Zeen ||
			previousIndexLetter ==
			(int)IsolatedLetters.PersianZe ||
			previousIndexLetter == (int)IsolatedLetters.Waw ||
			previousIndexLetter ==
			(int)IsolatedLetters.AlefMad ||
			previousIndexLetter ==
			(int)IsolatedLetters.AlefHamza ||
			previousIndexLetter ==
			(int)IsolatedLetters.Hamza ||
			previousIndexLetter ==
			(int)IsolatedLetters.AlefMaksoor;


			bool canThisLetterBeLeading = currentIndexLetter != ' ' &&
			currentIndexLetter != (int)GeneralLetters.Dal &&
			currentIndexLetter != (int)GeneralLetters.Thal &&
			currentIndexLetter != (int)GeneralLetters.Ra2 &&
			currentIndexLetter != (int)GeneralLetters.Zeen &&
			currentIndexLetter != (int)GeneralLetters.PersianZe &&
			currentIndexLetter != (int)GeneralLetters.Alef &&
			currentIndexLetter != (int)GeneralLetters.AlefHamza &&
			currentIndexLetter != (int)GeneralLetters.AlefMaksoor &&
			currentIndexLetter != (int)GeneralLetters.AlefMad &&
			currentIndexLetter != (int)GeneralLetters.WawHamza &&
			currentIndexLetter != (int)GeneralLetters.Waw &&
			currentIndexLetter !=
			(int)GeneralLetters.ZeroWidthNoJoiner &&
			currentIndexLetter != (int)GeneralLetters.Hamza;

			bool isNextLetterConnectable = index < letters.Length - 1 &&
			TextUtils.IsRTLCharacter(nextIndexLetter) &&
			nextIndexLetter != (int)GeneralLetters.Hamza &&
			nextIndexLetter !=
			(int)GeneralLetters.ZeroWidthNoJoiner;

			return isPreviousLetterNonConnectable &&
			canThisLetterBeLeading &&
			isNextLetterConnectable;
		}

		/// <summary>
		///     Is the letter at provided index a finishing letter?
		/// </summary>
		/// <returns><see langword="true" /> if the letter is a finishing letter</returns>
		private static bool IsFinishingLetter (FastStringBuilder letters, int index)
		{
			char currentIndexLetter = letters.Get(index);

			char previousIndexLetter = default;
			if (index != 0)
				previousIndexLetter = letters.Get(index - 1);

			bool isPreviousLetterConnectable = index != 0 &&
			previousIndexLetter != ' ' &&
			previousIndexLetter != (int)GeneralLetters.Dal &&
			previousIndexLetter != (int)GeneralLetters.Thal &&
			previousIndexLetter != (int)GeneralLetters.Ra2 &&
			previousIndexLetter != (int)GeneralLetters.Zeen &&
			previousIndexLetter != (int)GeneralLetters.PersianZe &&
			previousIndexLetter != (int)GeneralLetters.Waw &&
			previousIndexLetter != (int)GeneralLetters.Alef &&
			previousIndexLetter != (int)GeneralLetters.AlefMad &&
			previousIndexLetter != (int)GeneralLetters.AlefHamza &&
			previousIndexLetter != (int)GeneralLetters.AlefMaksoor &&
			previousIndexLetter != (int)GeneralLetters.WawHamza &&
			previousIndexLetter != (int)GeneralLetters.Hamza &&
			previousIndexLetter != (int)GeneralLetters.ZeroWidthNoJoiner &&
			previousIndexLetter != (int)IsolatedLetters.Dal &&
			previousIndexLetter != (int)IsolatedLetters.Thal &&
			previousIndexLetter != (int)IsolatedLetters.Ra2 &&
			previousIndexLetter != (int)IsolatedLetters.Zeen &&
			previousIndexLetter != (int)IsolatedLetters.PersianZe &&
			previousIndexLetter != (int)IsolatedLetters.Waw &&
			previousIndexLetter != (int)IsolatedLetters.Alef &&
			previousIndexLetter != (int)IsolatedLetters.AlefMad &&
			previousIndexLetter != (int)IsolatedLetters.AlefHamza &&
			previousIndexLetter != (int)IsolatedLetters.AlefMaksoor &&
			previousIndexLetter != (int)IsolatedLetters.WawHamza &&
			previousIndexLetter != (int)IsolatedLetters.Hamza &&
			TextUtils.IsRTLCharacter(previousIndexLetter);


			bool canThisLetterBeFinishing = currentIndexLetter != ' ' &&
			currentIndexLetter != (int)GeneralLetters.ZeroWidthNoJoiner &&
			currentIndexLetter != (int)GeneralLetters.Hamza;

			return isPreviousLetterConnectable && canThisLetterBeFinishing;
		}

		/// <summary>
		///     Is the letter at provided index a middle letter?
		/// </summary>
		/// <returns><see langword="true" /> if the letter is a middle letter</returns>
		private static bool IsMiddleLetter (FastStringBuilder letters, int index)
		{
			var currentIndexLetter = letters.Get(index);

			char previousIndexLetter = default;
			if (index != 0)
				previousIndexLetter = letters.Get(index - 1);

			char nextIndexLetter = default;
			if (index < letters.Length - 1)
				nextIndexLetter = letters.Get(index + 1);

			bool middleLetterCheck = index != 0 &&
			currentIndexLetter != (int)GeneralLetters.Alef &&
			currentIndexLetter != (int)GeneralLetters.Dal &&
			currentIndexLetter != (int)GeneralLetters.Thal &&
			currentIndexLetter != (int)GeneralLetters.Ra2 &&
			currentIndexLetter != (int)GeneralLetters.Zeen &&
			currentIndexLetter != (int)GeneralLetters.PersianZe &&
			currentIndexLetter != (int)GeneralLetters.Waw &&
			currentIndexLetter != (int)GeneralLetters.AlefMad &&
			currentIndexLetter != (int)GeneralLetters.AlefHamza &&
			currentIndexLetter != (int)GeneralLetters.AlefMaksoor &&
			currentIndexLetter != (int)GeneralLetters.WawHamza &&
			currentIndexLetter != (int)GeneralLetters.ZeroWidthNoJoiner &&
			currentIndexLetter != (int)GeneralLetters.Hamza;

			bool previousLetterCheck = index != 0 &&
			previousIndexLetter != (int)GeneralLetters.Alef &&
			previousIndexLetter != (int)GeneralLetters.Dal &&
			previousIndexLetter != (int)GeneralLetters.Thal &&
			previousIndexLetter != (int)GeneralLetters.Ra2 &&
			previousIndexLetter != (int)GeneralLetters.Zeen &&
			previousIndexLetter != (int)GeneralLetters.PersianZe &&
			previousIndexLetter != (int)GeneralLetters.Waw &&
			previousIndexLetter != (int)GeneralLetters.AlefMad &&
			previousIndexLetter != (int)GeneralLetters.AlefHamza &&
			previousIndexLetter != (int)GeneralLetters.AlefMaksoor &&
			previousIndexLetter != (int)GeneralLetters.WawHamza &&
			previousIndexLetter != (int)GeneralLetters.Hamza &&
			previousIndexLetter !=
			(int)GeneralLetters.ZeroWidthNoJoiner &&
			previousIndexLetter != (int)IsolatedLetters.Alef &&
			previousIndexLetter != (int)IsolatedLetters.Dal &&
			previousIndexLetter != (int)IsolatedLetters.Thal &&
			previousIndexLetter != (int)IsolatedLetters.Ra2 &&
			previousIndexLetter != (int)IsolatedLetters.Zeen &&
			previousIndexLetter != (int)IsolatedLetters.PersianZe &&
			previousIndexLetter != (int)IsolatedLetters.Waw &&
			previousIndexLetter != (int)IsolatedLetters.AlefMad &&
			previousIndexLetter != (int)IsolatedLetters.AlefHamza &&
			previousIndexLetter != (int)IsolatedLetters.AlefMaksoor &&
			previousIndexLetter != (int)IsolatedLetters.WawHamza &&
			previousIndexLetter != (int)IsolatedLetters.Hamza &&
			TextUtils.IsRTLCharacter(previousIndexLetter);

			bool nextLetterCheck = index < letters.Length - 1 &&
			TextUtils.IsRTLCharacter(nextIndexLetter) &&
			nextIndexLetter != (int)GeneralLetters.ZeroWidthNoJoiner &&
			nextIndexLetter != (int)GeneralLetters.Hamza &&
			nextIndexLetter != (int)IsolatedLetters.Hamza;

			return nextLetterCheck && previousLetterCheck && middleLetterCheck;
		}
	}
}
