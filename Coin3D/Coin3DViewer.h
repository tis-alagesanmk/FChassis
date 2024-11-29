#pragma once
#include <Inventor/Win/SoWin.h>
#include <Inventor/Win/SoWinRenderArea.h>
#include <Inventor/Win/viewers/SoWinExaminerViewer.h>
#include <Inventor/nodes/SoSeparator.h>
#include <Inventor/nodes/SoPerspectiveCamera.h>
#include <Inventor/nodes/SoOrthographicCamera.h>
#include <Inventor/nodes/SoDirectionalLight.h>
#include <Inventor/nodes/SoMaterial.h>

#include <Inventor/nodes/SoSphere.h>
#include <Inventor/nodes/SoCone.h>
#include <Inventor/nodes/SoTranslation.h>
using namespace System;

namespace Coin3D { namespace Inventor {
public ref class Viewer
{
public: 
	IntPtr Create(IntPtr _hwnd)
	{
		HWND hWnd = reinterpret_cast<HWND>(_hwnd.ToPointer());
		SoWin::init(hWnd);

		// Create a scene containing a sphere
		SoSeparator* root = new SoSeparator;
		root->ref(); // increment the root's reference counter

		SoPerspectiveCamera* camera = new SoPerspectiveCamera;
		SoOrthographicCamera* ocamera = new SoOrthographicCamera;
		SoDirectionalLight* light = new SoDirectionalLight;
		SoMaterial* material = new SoMaterial;
		SoSphere* sphere = new SoSphere;
		SoCone* cone = new SoCone;

		root->addChild(ocamera); // add camera node to the scene graph
		root->addChild(light); // add directional light to the scene
		root->addChild(material); // add material (with default settings)

		root->addChild(sphere); // add sphere node to the scene graph

		SoTranslation* translation = new SoTranslation;
		translation->translation.setValue(2.0f, 0.0f, 0.0f);  // Translate by (2, 0, 0)
		root->addChild(translation);

		SoMaterial* material1 = new SoMaterial;
		material1->ambientColor.setValue(0, 1, 0);
		root->addChild(material1);                         // <<< add Material before Cone

		root->addChild(cone); // add cone node to the scene graph

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

protected:
	//SoWinRenderArea* renderarea = NULL;
	SoWinExaminerViewer* renderarea = NULL;
};

}}
