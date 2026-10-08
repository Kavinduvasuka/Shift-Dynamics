package com.example.myapplication;

import org.junit.Test;
import org.json.JSONObject;

import static org.junit.Assert.*;

public class DomainTest {
    @Test
    public void phoneAddressUsesComputerOrigin() throws Exception {
        assertEquals("http://192.168.8.10:5174", Domain.address("http://192.168.8.10:5174/", true));
    }

    @Test
    public void releaseRequiresHttps() throws Exception {
        try {
            Domain.address("http://192.168.8.10:5174", false);
            fail();
        } catch (Exception expected) {
        }
        assertEquals("https://garage.example.com", Domain.address("https://garage.example.com", false));
    }

    @Test
    public void rejectsWrongOrigins() throws Exception {
        for (String url : new String[]{"http://localhost:5174", "http://127.0.0.1:5174", "http://user:pass@192.168.8.10:5174", "http://192.168.8.10:5174/api", "http://192.168.8.10:5174?x=1"})
            try {
                Domain.address(url, true);
                fail(url);
            } catch (Exception expected) {
            }
    }

    @Test
    public void loginAcceptsOnlyTwoRoles() throws Exception {
        for (String role : new String[]{"ServiceAdvisor", "Mechanic"}) {
            Session.login(Domain.json("role", role, "accessToken", "token", "fullName", "Real User"), "http://192.168.8.10:5174");
            assertEquals(role, Session.role);
            assertEquals("Real User", Session.name);
            Session.clear();
        }
        for (String role : new String[]{"Customer", "Manager", "Vendor", "Admin"})
            try {
                Session.login(Domain.json("role", role, "accessToken", "token"), "http://192.168.8.10:5174");
                fail(role);
            } catch (Exception expected) {
            }
        assertEquals("", Session.token);
    }

    @Test
    public void assignmentIdDoesNotReplaceWorkOrderId() {
        assertEquals("job-id", Domain.id(Domain.json("id", "assignment-id", "workOrderId", "job-id")));
    }

    @Test
    public void approvedEstimateMustBelongToJob() {
        JSONObject ok = Domain.json("status", "Approved", "workOrderId", "job-A");
        assertTrue(Domain.approvedForJob(ok, "job-A"));
        assertFalse(Domain.approvedForJob(ok, "job-B"));
        assertFalse(Domain.approvedForJob(Domain.json("status", 1, "workOrderId", "job-A"), "job-A"));
        assertTrue(Domain.approvedForJob(Domain.json("status", 2, "workOrderId", "job-A"), "job-A"));
    }

    @Test
    public void handoverCanReadPascalCaseBlock() {
        JSONObject block = Domain.block("{\"Notes\":\"Keys returned\",\"WorkComplete\":true}");
        assertEquals("Keys returned", Domain.value(block, "notes"));
        assertEquals(true, Domain.value(block, "workComplete"));
    }

    @Test
    public void descriptionHidesEmbeddedBlocks() {
        assertEquals("Repair brakes", Domain.strip("Repair brakes [WF:HANDOVER]{\"Notes\":\"Keys returned\"}[/WF:HANDOVER]"));
    }

    @Test
    public void moneyRejectsNegative() throws Exception {
        try {
            Domain.decimal("-1", java.math.BigDecimal.ZERO);
            fail();
        } catch (Exception expected) {
        }
        assertEquals("12.50", Domain.decimal("12.50", java.math.BigDecimal.ZERO).toPlainString());
    }
}