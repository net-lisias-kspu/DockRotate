/*
	This file is part of Dock Rotate /L Unleashed
		© 2021 LisiasT

	THIS FILE is licensed to you under:

	* WTFPL - http://www.wtfpl.net
		* Everyone is permitted to copy and distribute verbatim or modified
		  copies of this license document, and changing it is allowed as long
		  as the name is changed.

	THIS FILE is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*/
using System;
using System.Diagnostics;

#if DEBUG
using System.Collections.Generic;
#endif

using KSPe.Util.Log;

namespace DockRotate
{
	public static class Log
	{
		public interface IClient
		{
			string who(bool verbose);
		}

		private static readonly Logger log = Logger.CreateForType<Startup>();

		public static void force (string msg, params object [] @params)
		{
			log.force (msg, @params);
		}

		public static void info(string msg, params object[] @params)
		{
			log.info(msg, @params);
		}

		public static void warn(IClient who, string msg, params object[] @params) => warn(who, false, msg, @params);
		public static void warn(IClient who, bool verbose, string msg, params object[] @params)
		{
			if (log.level < Level.WARNING) return;	// Saving some CPU juice due the string.Format below
			log.warn(string.Format("{0} {1}", who.who(verbose), msg), @params);
		}
		public static void warn(Part who, string msg, params object[] @params)
		{
			if (log.level < Level.WARNING) return;	// Saving some CPU juice due the string.Format below
			log.warn(string.Format("{0} {1}", who.desc(), msg), @params);
		}
		public static void warn(Type who, string msg, params object[] @params)
		{
			if (log.level < Level.WARNING) return;	// Saving some CPU juice due the string.Format below
			log.warn(string.Format("{0} {1}", who.Name, msg), @params);
		}

		public static void detail(IClient who, string msg, params object[] @params) => detail(who, false, msg, @params);
		public static void detail(IClient who, bool verbose, string msg, params object[] @params)
		{
			if (log.level < Level.DETAIL) return;	// Saving some CPU juice due the string.Format below
			log.detail(string.Format("{0} {1}", who.who(verbose), msg), @params);
		}
		public static void detail(AttachNode who, string msg, params object[] @params)
		{
			if (log.level < Level.DETAIL) return;	// Saving some CPU juice due the string.Format below
			log.detail(string.Format("{0} {1}", who.desc(), msg), @params);
		}
		public static void detail(Part who, string msg, params object[] @params)
		{
			if (log.level < Level.DETAIL) return;	// Saving some CPU juice due the string.Format below
			log.detail(string.Format("{0} {1}", who.desc(), msg), @params);
		}
		public static void detail(PartJoint who, string msg, params object[] @params)
		{
			if (log.level < Level.DETAIL) return;	// Saving some CPU juice due the string.Format below
			log.detail(string.Format("{0} {1}", who.desc(), msg), @params);
		}
		public static void detail(Type who, string msg, params object[] @params)
		{
			if (log.level < Level.DETAIL) return;	// Saving some CPU juice due the string.Format below
			log.detail(string.Format("{0} {1}", who.Name, msg), @params);
		}

		public static void trace(IClient who, string msg, params object[] @params) => trace(who, false, msg, @params);
		public static void trace(IClient who, bool verbose, string msg, params object[] @params)
		{
			if (log.level < Level.TRACE) return;	// Saving some CPU juice due the string.Format below
			log.trace(string.Format("{0} {1}", who.who(verbose), msg), @params);
		}
		public static void trace(Part who, string msg, params object[] @params)
		{
			if (log.level < Level.TRACE) return;	// Saving some CPU juice due the string.Format below
			log.trace(string.Format("{0} {1}", who.desc(), msg), @params);
		}
		public static void trace(string msg, params object[] @params) => log.trace(string.Format("{0} {1}", @params));

		public static void error(Exception e, object offended)
		{
			log.error(offended, e);
		}

		public static void error(Exception e, string msg, params object[] @params)
		{
			log.error(e, msg, @params);
		}

		public static void error(string msg, params object[] @params)
		{
			log.error(msg, @params);
		}

		[ConditionalAttribute("DEBUG")]
		public static void dbg(IClient who, string msg, params object[] @params) => trace(who, false, msg, @params);
		[ConditionalAttribute("DEBUG")]
		public static void dbg(IClient who, bool verbose, string msg, params object[] @params)
		{
			if (log.level < Level.TRACE) return;	// Saving some CPU juice due the string.Format below
			log.dbg(string.Format("{0} {1}", who.who(verbose), msg), @params);
		}
		[ConditionalAttribute("DEBUG")]
		public static void dbg(string msg, params object[] @params) => log.trace(msg, @params);

		#if DEBUG
		private static readonly HashSet<string> DBG_SET = new HashSet<string>();
		#endif

		[ConditionalAttribute("DEBUG")]
		public static void dbgOnce(string msg, params object[] @params)
		{
			string new_msg = string.Format(msg, @params);
			#if DEBUG
			if (DBG_SET.Contains(new_msg)) return;
			DBG_SET.Add(new_msg);
			#endif
			log.trace(new_msg);
		}

		public static void evlog(IClient who, string name, Vessel v, bool care) => evlog(who, name, v.desc(), care);
		public static void evlog(IClient who, string name, Part p, bool care) => evlog(who, name, p.desc(), care);
		public static void evlog(IClient who, string name, GameEvents.FromToAction<Part, Part> action, bool care) => evlog(who, name, action.from.desc() + ", " + action.to.desc(), care);
		public static void evlog(IClient who, string name, GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> action, bool care) => evlog(who, name, action.from.part.desc() + ", " + action.to.part.desc(), care);
		public static void evlog(IClient who, string name, uint id1, uint id2, bool care) => evlog(who, name, id1 + ", " + id2, care);
		public static void evlog(IClient who, string name, string desc, bool care) => Log.trace(who.who(true), ": *** EVENT *** {0}({1}), {2}", name, desc, (care ? "care" : "don't care"));
	}
}
