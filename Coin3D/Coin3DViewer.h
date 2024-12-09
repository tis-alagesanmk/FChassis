#pragma once
#include <Inventor/Win/SoWin.h>
#include <Inventor/Win/SoWinRenderArea.h>
#include <Inventor/Win/viewers/SoWinExaminerViewer.h>
#include <Inventor/nodes/SoSeparator.h>
#include <Inventor/nodes/SoPerspectiveCamera.h>
#include <Inventor/nodes/SoOrthographicCamera.h>
#include <Inventor/nodes/SoDirectionalLight.h>
#include <Inventor/nodes/SoMaterial.h>
#include <Inventor/nodes/SoCoordinate3.h>
#include <Inventor/nodes/SoLineSet.h>
#include <Inventor/nodes/SoQuadMesh.h>

#include <Inventor/nodes/SoSphere.h>
#include <Inventor/nodes/SoCone.h>
#include <Inventor/nodes/SoTranslation.h>

using namespace System;
using namespace FChassis::Core::Drawing;
using namespace Flux::API;

class GCodeDrawing { 
public:
	typedef void (*FNCreateShape)(SoSeparator* pSep, int count);
	void createShape (SoSeparator* root, SoSeparator*& container,
					  SoMaterial*& rpMaterial, SoCoordinate3*& rpCoord3,
					  FNCreateShape pCreateShapeFN, int count = 0)	{ 
		//assert(pCreateShape);
		SoSeparator* pSep = container;
		if (nullptr == pSep) {
			pSep = new SoSeparator();

			rpMaterial = new SoMaterial;
			pSep->addChild(rpMaterial);

			rpCoord3 = new SoCoordinate3();
			pSep->addChild(rpCoord3);

			pCreateShapeFN(pSep, count);
			 
			root->addChild(pSep);
			container = pSep;
		}
		else {
			rpMaterial = (SoMaterial*)pSep->getChild(0);
			rpCoord3 = (SoCoordinate3*)pSep->getChild(1);
		}
	}

	void UpdateSepDraw(SoSeparator* root, SoSeparator** ppArray, int index, Color32 color,
				       Point3List^ pts, Point3ListList^ ptsList = nullptr, 
					   Point3List^ ptQuadList = nullptr, double height = 0) {
		SoMaterial* pMaterial; SoCoordinate3* pCoord3;

		if(nullptr != pts || nullptr != ptsList)
			this->createShape(root, ppArray[index], pMaterial, pCoord3, _createLineSet);

		this->_setColor(pMaterial, color);
		if (nullptr != pts)
			this->_drawLines(pCoord3, pts);

		if (nullptr != ptsList)
			for each (auto pts in ptsList)
				this->_drawLines(pCoord3, pts);

		if (nullptr != ptQuadList) {
			this->createShape(root, ppArray[index], pMaterial, pCoord3, _createQuadMesh);
			this->_drawQuads(pCoord3, ptQuadList, height);
		}
	}

	void UpdateGCodeLines(SoSeparator* root, int index, Color32 color,
					     Point3List^ pts, Point3ListList^ ptsList) {
		UpdateSepDraw(root, this->lineSegs, index, color, pts, ptsList); }

	void UpdateToolWayPoints(SoSeparator* root, Color32 lineColor, Color32 quadColor,
						     Point3List^ ptList, double height) {
		// Draw Line
		UpdateSepDraw(root, this->wayPoints, 0, lineColor, ptList);

		// Draw Quad
		UpdateSepDraw(root, this->wayPoints, 1, quadColor, ptList,
					  nullptr, nullptr, height);
	}

	void UpdateSegs(SoSeparator* root, Color32 color,
					Point3List^ ptList) {
		// Draw Line
		UpdateSepDraw(root, this->segs, 0, color, ptList); }

	static void _createLineSet(SoSeparator* pSep, int count) {
		SoLineSet* pLineSet = new SoLineSet();
		pSep->addChild(pLineSet); }

	static void _createQuadMesh(SoSeparator* pSep, int count) {
		SoQuadMesh* mesh = new SoQuadMesh();

		mesh->verticesPerRow = count;
		mesh->verticesPerColumn = 5;

		pSep->addChild(mesh); }

	void _drawLines (SoCoordinate3* pCoord3, Point3List^ pts) {
		if (pts->Count <= 0)
			return;

		int p = 0;
		float(*xyzs)[3] = new float[pts->Count + 1][3];
		for each (auto pt in pts)
			_updateValues(xyzs[p++], pt);

		_updateValues(xyzs[0], pts[0]);

		pCoord3->point.setValues(0, pts->Count + 1, xyzs);
		delete[] xyzs;		
	}

	void _updateValues (float* xyzs, Point3 pt) {
		float* xyz = xyzs;
		*xyz++ = (float)pt.X;
		*xyz++ = (float)pt.Y;
		*xyz++ = (float)pt.Z;
	}

	void _drawQuads(SoCoordinate3* pCoord3, Point3List^ ptList, double height) {
		if (ptList->Count <= 0)
			return;

		int p = 0;
		Point3^ pt = gcnew Point3();
		Point3^ pt0;
		Point3^ pt1;
		float(*xyzs)[3] = new float[ptList->Count * 5][3];
		for each (auto _pt in ptList) {
			pt1 = pt;
			if (p > 0) {
				_updateValues(xyzs[p++], pt0);
				_updateValues(xyzs[p++], pt1);

				pt = *(gcnew Point3 (pt1->X + pt1->X * height,
									 pt1->Y + pt1->Y * height,
									 pt1->Z + pt1->Z * height));
				_updateValues(xyzs[p++], pt);

				pt = *(gcnew Point3(pt0->X + pt0->X * height,
									pt0->Y + pt0->Y * height,
									pt0->Z + pt0->Z * height));
				_updateValues(xyzs[p++], pt);

				_updateValues(xyzs[p++], pt0);
			}
			
			pt0 = pt1; 
		}		

		pCoord3->point.setValues(0, ptList->Count, xyzs);
		delete[] xyzs;
	}

	void _updateValues(float* xyzs, Point3^ pt) {
		float* xyz = xyzs;
		*xyz++ = (float)pt->X;
		*xyz++ = (float)pt->Y;
		*xyz++ = (float)pt->Z;
	}

	void _setColor(SoMaterial* pMaterial, Color32 color) { 
		pMaterial->ambientColor.setValue(color.R / 255.0f,
			color.G / 255.0f,
			color.B / 255.0f); }

public: 
	SoSeparator* lineSegs[4] = {nullptr, nullptr, nullptr, nullptr};
	SoSeparator* wayPoints[2] = { nullptr };
	SoSeparator* segs[1] = { nullptr };
};

namespace Coin3D { namespace Inventor {
public ref class Viewer
{
public: 
	IntPtr Create(IntPtr _hwnd) {
		HWND hWnd = reinterpret_cast<HWND>(_hwnd.ToPointer());
		SoWin::init(hWnd);

		// Create a scene containing a sphere
		this->root = new SoSeparator;
		this->root->ref(); // increment the root's reference counter

		SoPerspectiveCamera* camera = new SoPerspectiveCamera;
		SoOrthographicCamera* ocamera = new SoOrthographicCamera;
		SoDirectionalLight* light = new SoDirectionalLight;

		this->root->addChild(ocamera);	// add camera node to the scene graph
		this->root->addChild(light);    // add directional light to the scene

		/*
		SoMaterial* material = new SoMaterial;
		this->root->addChild(material); // add material (with default settings)

		SoSphere* sphere = new SoSphere;
		this->root->addChild(sphere);	// add sphere node to the scene graph

		SoTranslation* translation = new SoTranslation;
		translation->translation.setValue(2.0f, 0.0f, 0.0f);
		this->root->addChild(translation);

		SoMaterial* material1 = new SoMaterial;
		material1->ambientColor.setValue(0, 1, 0);
		this->root->addChild(material1);

		SoCone* cone = new SoCone;
		this->root->addChild(cone); // add cone node to the scene graph
		*/

		// Create a renderingarea which will be used to display the
		// scene graph in the window.
		this->renderarea = new SoWinExaminerViewer(hWnd);

		// Make the camera able to see the whole scene
		camera->viewAll(root, this->renderarea->getViewportRegion());

		// Display the scene in our renderarea and change the title
		this->renderarea->setSceneGraph(root);
		this->renderarea->setTitle("Sphere");
		this->renderarea->show();

		HWND hViewer = this->renderarea->getWidget();
		IntPtr handle = IntPtr(reinterpret_cast<void*>(hViewer));
		return handle;
	}

	void UpdateGCodeLines(int index, Color32 color, Point3List^ pts, Point3ListList^ ptsList) {
		this->gcodeDrawing->UpdateGCodeLines(this->root, index, color, pts, ptsList); }

	void UpdateToolWayPoints(Color32 lineColor, Color32 quadColor, 
							 Point3List^ ptList, double height) {
		this->gcodeDrawing->UpdateToolWayPoints(this->root, lineColor, quadColor, 
												ptList, height); }

	void UpdateSegs(Color32 color, Point3List^ ptList) {
		this->gcodeDrawing->UpdateSegs(this->root, color, ptList); }

protected:
	SoWinExaminerViewer* renderarea = NULL;
	GCodeDrawing* gcodeDrawing = new GCodeDrawing;
	SoSeparator* root = nullptr;
};

} } // Namespace
