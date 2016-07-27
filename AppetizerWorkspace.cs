/*
 * Created by Dennis M. Heine 
 * Date: 27.07.2016
 * Description: This program resizes the desktop workspace for Appetizer.
 *              Changing the CLASSNAME const allows use with nearly any other toolbar.
 * 
 *  Copyright 2016 Dennis M. Heine
 *   This file is part of the Appetizer Workspace Plugin.
 *
 *   Appetizer Workspace Plugin is free software: you can redistribute it and/or modify it under the terms 
 *   of the GNU General Public License as published by the Free Software Foundation, either version 3 of the 
 *   License, or (at your option) any later version.
 *   Appetizer Workspace Plugin is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 *   without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *   See the GNU General Public License for more details.
 *   You should have received a copy of the GNU General Public License along with Appetizer Workspace Plugin.
 *   If not, see http://www.gnu.org/licenses/.  
 */
 
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace AppetizerWorkspace
{	
	public sealed class AppetizerWorkspace
	{				
		private const String CLASSNAME="wxWindowClassNR";
		
		[DllImport("user32.dll")]
		private static extern int FindWindow(string sClass, string sWindow);				
		 
		[DllImport("user32.dll")]
		private static extern bool SystemParametersInfo(int uiAction, int uiParam, ref RCT pvParam, int fWinIni);

		[DllImport("shell32.dll")]
	    private static extern IntPtr SHAppBarMessage(int msg, ref APPBARDATA data);		
	    
		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hwnd, ref Rectangle rectangle);	    
				
		private const Int32 SPIF_SENDWININICHANGE = 2;
		private const Int32 SPIF_UPDATEINIFILE = 1;
		private const Int32 SPIF_change = SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE;
		private const Int32 SPI_SETWORKAREA = 47;
		private const Int32 SPI_GETWORKAREA = 48;		
		private const Int32 ABM_GETTASKBARPOS = 5;				
				
		private struct RCT
		{
		    public Int32 Left;
		    public Int32 Top;
		    public Int32 Right;
		    public Int32 Bottom;
		}
			    	    
	    private struct APPBARDATA {
	        public int cbSize;
	        public IntPtr hWnd;
	        public int uCallbackMessage;
	        public int uEdge;
	        public RCT rc;
	        public IntPtr lParam;
	    }
	    										
		private enum Pos{	
			NONE=0,
			BOTTOM=1,
			TOP=2,
			RIGHT=3,
			LEFT=4,	
		}
		
		
		private static bool SetWorkspace(RCT rect)
		{
		   bool result = SystemParametersInfo(SPI_SETWORKAREA, 0, ref rect, SPIF_change);		   		
		   return result;
		}
		
		private static Rectangle GetTaskbarPosition() {
	        var data = new APPBARDATA();
	        data.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(data);
	        IntPtr retval = SHAppBarMessage(ABM_GETTASKBARPOS, ref data);
	        
	        return new Rectangle(data.rc.Left, data.rc.Top,
	            data.rc.Right - data.rc.Left, data.rc.Bottom - data.rc.Top);	        	
	    }    											
		
		private static Pos getTaskBarPos()
		{
			Pos ret=Pos.NONE;
			Rectangle tb=GetTaskbarPosition();
			if(tb.Left==0 && tb.Top > 0)
				ret=Pos.BOTTOM;
			else if(tb.Left==0 && tb.Top ==0 && tb.Width<500)
				ret=Pos.LEFT;
			else if(tb.Left>0)
				ret=Pos.RIGHT;
			else
				ret=Pos.TOP;
			
			return ret;
		}		
		
		private static Rectangle getAppetizerPos()
		{
					Rectangle rct=new Rectangle();
					int nWinHandle = FindWindow(CLASSNAME,null);									
					IntPtr pWin=new IntPtr(nWinHandle);
					GetWindowRect(pWin, ref rct);			
					return rct;
		}
		
		private static void SetWorkspaceOutside()
		{		
			
			RCT r=new RCT();
			Pos tbPos=getTaskBarPos();	
			
			if(tbPos==Pos.LEFT)
			{
				r.Top=0;
				r.Bottom=Screen.GetBounds(new Point (500,500)).Bottom;		
				r.Right=Screen.GetBounds(new Point (500,500)).Right;
				r.Left=getAppetizerPos().Width+GetTaskbarPosition().Width-getAppetizerPos().Left;
			}			
			if(tbPos==Pos.RIGHT)
			{
				r.Top=0;
				r.Bottom=Screen.GetBounds(new Point (500,500)).Bottom;		
				r.Right=getAppetizerPos().Left;		
				r.Left=0;
			}				
			if(tbPos==Pos.TOP)
			{
				r.Bottom=Screen.GetBounds(new Point (500,500)).Bottom;
				r.Top=getAppetizerPos().Bottom-getAppetizerPos().Top;
					
				r.Left=5;
				r.Right=Screen.GetBounds(new Point(0,0)).Width-5;
			}
			else if(tbPos==Pos.BOTTOM)
			{
				r.Bottom=getAppetizerPos().Top;
				r.Top=0;		
					
				r.Left=5;
				r.Right=Screen.GetBounds(new Point(0,0)).Width-5;
			}
			
			SetWorkspace(r);	
		}		
		
		private static void SetWorkspaceBetween()
		{
			Pos tbPos=getTaskBarPos();	
			RCT r=new RCT();
			
			
			
			if(tbPos==Pos.BOTTOM)
			{
				r.Top=getAppetizerPos().Bottom-getAppetizerPos().Top;
				r.Bottom=GetTaskbarPosition().Top;
				
				r.Left=5;
				r.Right=Screen.GetBounds(new Point(0,0)).Width;
			}
			else if(tbPos==Pos.TOP)
			{
				r.Top=GetTaskbarPosition().Bottom;
				r.Bottom=getAppetizerPos().Top;
				
				r.Left=5;
				r.Right=Screen.GetBounds(new Point(0,0)).Width;
			}
			else if(tbPos==Pos.LEFT)
			{
				r.Top=0;
				r.Bottom=Screen.GetBounds(new Point(0,0)).Bottom;
				
				r.Left=GetTaskbarPosition().Width;
				r.Right=getAppetizerPos().Left;
			}
			else if(tbPos==Pos.RIGHT)
			{		
				r.Top=0;
				r.Bottom=Screen.GetBounds(new Point(0,0)).Bottom;
				
				r.Left=getAppetizerPos().Width;
				r.Right=GetTaskbarPosition().Left;
			}
			SetWorkspace(r);
		}
	
	
		[STAThread]
		public static void Main(string[] args)
		{			
			try{				
				Pos tbPos=getTaskBarPos();							
				if((tbPos==Pos.TOP && getAppetizerPos().Top-GetTaskbarPosition().Bottom<=100) || (tbPos==Pos.BOTTOM && GetTaskbarPosition().Top-getAppetizerPos().Bottom<=100)
				   || (tbPos==Pos.LEFT && getAppetizerPos().Left-GetTaskbarPosition().Width <=100) || (tbPos==Pos.RIGHT && GetTaskbarPosition().Left-getAppetizerPos().Right<=100))
					SetWorkspaceOutside();
				else
					SetWorkspaceBetween();					
			}catch(Exception e){}
		}
	}
}
